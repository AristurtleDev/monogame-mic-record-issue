using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace MicTest
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private enum ProgramState
        {
            None,
            Started,
            Stopped,
            Playing
        }

        private GraphicsDeviceManager graphics;
        private SpriteBatch _spriteBatch;

        //  Font used to render text to screen
        private SpriteFont _font;

        //  Just using the default detected microphone, ensure you have yours
        //  setup appropriatly on your PC
        private Microphone _mic = Microphone.Default;

        //  Buffer to hold the data from microphone when buffer is ready.
        private byte[] _buffer;
        
        private int _lastDataSize;

        //  Stream that the buffer will be written to as we record sound
        private MemoryStream _stream = new MemoryStream();

        //  SoundEffect instance used to create the SoundEffectInstance that will
        //  play the recorded audio back.
        private SoundEffect _soundEffect;

        //  The SoundEffectInstance that is created to play the recorded audio back
        private SoundEffectInstance _soundEffectInstance;

        //  Track the previous and current mouse states
        private MouseState _prevMouseState;
        private MouseState _curMouseState;

        //  Track the state of the program
        private ProgramState _programState;



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //  Register the BufferReady event
            _mic.BufferReady += OnBufferReady;
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //  Load the font file so we can output text
            _font = Content.Load<SpriteFont>("font");

        }

        protected override void Update(GameTime gameTime)
        {
            //  Update mouse state information
            _prevMouseState = _curMouseState;
            _curMouseState = Mouse.GetState();



            switch (_programState)
            {
                //  When the program state is None, pressing the Left Mouse Button will start
                //  the audio recording from the mic.
                case ProgramState.None:
                    if (_prevMouseState.LeftButton == ButtonState.Released && _curMouseState.LeftButton == ButtonState.Pressed)
                    {
                        _programState = ProgramState.Started;
                        Start();
                    }
                    break;

                //  When the program state is "Started", pressing the Right Mouse Button will stop
                //  the audio recording from the mic
                case ProgramState.Started:
                    if (_prevMouseState.RightButton == ButtonState.Released && _curMouseState.RightButton == ButtonState.Pressed)
                    {
                        _programState = ProgramState.Stopped;
                        Stop();
                    }
                    break;

                //  When the program state is "Stopped", pressing the Middle Mouse Button will play
                //  the recorded audio.
                case ProgramState.Stopped:
                    if (_prevMouseState.MiddleButton == ButtonState.Released && _curMouseState.MiddleButton == ButtonState.Pressed)
                    {
                        _programState = ProgramState.Playing;
                        Play();
                    }
                    break;
            }

            //  When the SoundEffectInstance isn't null and has finished playing, null it out and set the
            //  program state to None.
            if (_soundEffectInstance != null && _soundEffectInstance.State == SoundState.Stopped)
            {
                _soundEffectInstance = null;
                _programState = ProgramState.None;
            }


            base.Update(gameTime);
        }

        //  Called when Microphone.OnBufferReady is triggered
        //  Does a simple GetData of the Microphone and writes it to the stream.
        private void OnBufferReady(object sender, EventArgs e)
        {
            int dataSize = _mic.GetData(_buffer);
            _stream.Write(_buffer, 0, _buffer.Length);

            _lastDataSize = dataSize;
            System.Diagnostics.Debug.WriteLine("_lastDataSize: " + _lastDataSize);
        }

        //  Performs setup to start the microphone to begin recording audio.
        public void Start()
        {
            _mic.BufferDuration = TimeSpan.FromMilliseconds(100);
            _buffer = new byte[_mic.GetSampleSizeInBytes(_mic.BufferDuration)];
            _stream.SetLength(0);
            _mic.Start();
        }

        //  Performs the steps to stop the microphone from recording audio
        public void Stop()
        {
            if (_mic.State == MicrophoneState.Started)
            {
                _mic.Stop();
            }
        }

        //  Performs the steps to play the audio that was recorded by the microphone.
        public void Play()
        {
            if (_stream != null)
            {
                byte[] arr = _stream.ToArray();
                _soundEffect = new SoundEffect(arr, _mic.SampleRate, AudioChannels.Mono);
                _soundEffectInstance = _soundEffect.CreateInstance();
                _soundEffectInstance.Pitch = 0f;
                _soundEffectInstance.Volume = 1f;
                _soundEffectInstance.Play();
            }
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //  Just draw a string of the current program state to the screen somewhere.
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, "Program State:  " + _programState.ToString(), new Vector2(100, 100), Color.Black);
            _spriteBatch.DrawString(_font, "BufferDuration: " + _mic.BufferDuration.TotalSeconds.ToString(), new Vector2(100, 120), Color.Black);
            _spriteBatch.DrawString(_font, "Last DataSize:  " + _lastDataSize.ToString(), new Vector2(100, 140), Color.Black);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
