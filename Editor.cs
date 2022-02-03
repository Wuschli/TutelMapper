using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Barbar.HexGrid;
using Dcrew.Camera;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;
using MonoGame.ImGui.Standard;

namespace TutelMapper
{
    public class Editor : Game
    {
        private const float KeyboardCameraSpeed = 100f;
        private const float MouseZoomSpeed = 0.001f;
        private readonly Vector2 _minScale = new Vector2(0.1f);
        private readonly Vector2 _maxScale = new Vector2(3f);

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ImGUIRenderer _guiRenderer;

        private HexLayout<Vector2, Vector2Policy> _hexGrid;
        private Camera _camera;
        private Vector2 _previousMousePosition;
        private int _previousMouseScroll;
        private bool _isWindowHovered;
        private Texture2D _selectedTile = null;
        private string[,] _mapData = new string[10, 10];

        private readonly Dictionary<string, Texture2D> _tiles = new Dictionary<string, Texture2D>();
        private SpriteFont _font;


        private const string SDL = "SDL2.dll";
        private const float HexSize = 64f;

        [DllImport(SDL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SDL_MaximizeWindow(IntPtr window);

        public Editor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
        }

        protected override void Initialize()
        {
            _guiRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();

            _hexGrid = HexLayoutFactory.CreateFlatHexLayout<Vector2, Vector2Policy>(new Vector2(HexSize, HexSize), new Vector2(0, 0), Offset.Even);
            _camera = new Camera();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _font = Content.Load<SpriteFont>("font");

            // load tiles
            var tilesPath = Path.Combine("Content", "Tiles");
            foreach (var tileFile in Directory.EnumerateFiles(tilesPath, "*.png", SearchOption.AllDirectories))
            {
                var texture = Texture2D.FromFile(GraphicsDevice, tileFile);
                if (texture == null)
                    continue;
                var tileName = tileFile;
                if (Path.HasExtension(tileName))
                    tileName = tileName.Substring(0, tileName.Length - Path.GetExtension(tileFile).Length);
                tileName = tileName.Substring(tilesPath.Length + 1);
                texture.Name = tileName;
                _tiles.Add(tileName, texture);
            }

            SDL_MaximizeWindow(Window.Handle);
        }

        protected override void Update(GameTime gameTime)
        {
            _camera.UpdateMouseXY();

            if (!_isWindowHovered)
            {
                // Mouse Camera Movement
                var mouseState = Mouse.GetState();
                var camVector = Vector2.Zero;
                if (mouseState.MiddleButton == ButtonState.Pressed)
                    camVector += (_previousMousePosition - new Vector2(mouseState.X, mouseState.Y)) / _camera.Scale;
                _previousMousePosition = new Vector2(mouseState.X, mouseState.Y);

                var scrollDelta = mouseState.ScrollWheelValue - _previousMouseScroll;
                if (scrollDelta != 0)
                {
                    _camera.Scale += new Vector2(MouseZoomSpeed * scrollDelta);
                    _camera.Scale = Vector2.Clamp(_camera.Scale, _minScale, _maxScale);
                }

                _previousMouseScroll = mouseState.ScrollWheelValue;

                // Keyboard Camera Movement
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                    camVector.Y -= (float)gameTime.ElapsedGameTime.TotalSeconds * KeyboardCameraSpeed;
                if (Keyboard.GetState().IsKeyDown(Keys.S))
                    camVector.Y += (float)gameTime.ElapsedGameTime.TotalSeconds * KeyboardCameraSpeed;
                if (Keyboard.GetState().IsKeyDown(Keys.D))
                    camVector.X += (float)gameTime.ElapsedGameTime.TotalSeconds * KeyboardCameraSpeed;
                if (Keyboard.GetState().IsKeyDown(Keys.A))
                    camVector.X -= (float)gameTime.ElapsedGameTime.TotalSeconds * KeyboardCameraSpeed;

                _camera.XY += camVector;

                // Tile Placement
                if (_selectedTile != null)
                {
                    var hoveredHex = _hexGrid.PixelToHex(_camera.MouseXY).Round();
                    var offsetCoordinates = _hexGrid.ToOffsetCoordinates(hoveredHex);
                    if (mouseState.LeftButton == ButtonState.Pressed)
                        if (offsetCoordinates.Row >= 0 && offsetCoordinates.Column >= 0 && offsetCoordinates.Column < _mapData.GetLength(0) && offsetCoordinates.Row < _mapData.GetLength(1))
                        {
                            _mapData[offsetCoordinates.Column, offsetCoordinates.Row] = _selectedTile.Name;
                        }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(transformMatrix: _camera.View());

            // draw hex grid
            var hoveredHex = _hexGrid.PixelToHex(_camera.MouseXY).Round();
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                for (int j = 0; j < _mapData.GetLength(1); j++)
                {
                    var cubeCoordinates = _hexGrid.ToCubeCoordinates(new OffsetCoordinates(i, j));
                    var vertices = _hexGrid.PolygonCorners(cubeCoordinates);
                    var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;
                    for (int k = 0; k < 6; k++)
                    {
                        _spriteBatch.DrawLine(vertices[k], vertices[(k + 1) % 6], hovered ? Color.Red : Color.Beige, 1f);
                    }
                }
            }

            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                for (int j = 0; j < _mapData.GetLength(1); j++)
                {
                    var tileName = _mapData[i, j];
                    var cubeCoordinates = _hexGrid.ToCubeCoordinates(new OffsetCoordinates(i, j));
                    var hovered = hoveredHex.S == cubeCoordinates.S && hoveredHex.Q == cubeCoordinates.Q && hoveredHex.R == cubeCoordinates.R;

                    if (!string.IsNullOrEmpty(tileName))
                    {
                        var pixelCoordinates = _hexGrid.HexToPixel(cubeCoordinates);
                        if (_tiles.TryGetValue(tileName, out var tileTexture))
                        {
                            _spriteBatch.Draw(tileTexture, pixelCoordinates - new Vector2(HexSize), null, Color.White, 0f, Vector2.Zero, new Vector2(HexSize / tileTexture.Width * 2f, HexSize / tileTexture.Height * 2f), SpriteEffects.None, 0f);
                        }
                        else
                        {
                            _spriteBatch.DrawString(_font, $"Tile not found!\n{tileName}", pixelCoordinates - new Vector2(HexSize / 2), Color.Red);
                        }
                    }

                    if (hovered && _selectedTile != null)
                    {
                        var pixelCoordinates = _hexGrid.HexToPixel(cubeCoordinates);
                        _spriteBatch.Draw(_selectedTile, pixelCoordinates - new Vector2(HexSize), null, Color.White, 0f, Vector2.Zero, new Vector2(HexSize / _selectedTile.Width * 2f, HexSize / _selectedTile.Height * 2f), SpriteEffects.None, 0f);
                    }
                }
            }

            _spriteBatch.End();

            // draw gui
            _guiRenderer.BeginLayout(gameTime);
            var menuBarHeight = ImGui.GetTextLineHeightWithSpacing() + 2;

            if (ImGui.BeginMainMenuBar())
            {
                menuBarHeight = ImGui.GetWindowHeight();
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Exit"))
                        Exit();
                    ImGui.EndMenu();
                }

                var fps = 1000 / gameTime.ElapsedGameTime.TotalMilliseconds;
                ImGui.LabelText($"{fps:F} fps ({gameTime.ElapsedGameTime.TotalMilliseconds:F}ms)", "");

                ImGui.EndMainMenuBar();
            }

            //ImGui.ShowDemoWindow();

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, menuBarHeight), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, GraphicsDevice.Viewport.Height - menuBarHeight), ImGuiCond.Always);
            ImGui.Begin("Tools", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
            foreach (var tile in _tiles.Values)
            {
                if (ImGui.Button(tile.Name))
                    _selectedTile = tile;
            }

            ImGui.End();

            _isWindowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);

            _guiRenderer.EndLayout();

            base.Draw(gameTime);
        }
    }

    public class Vector2Policy : IPointPolicy<Vector2>
    {
        public Vector2 Create(double x, double y)
        {
            return new Vector2((float)x, (float)y);
        }

        public double GetX(Vector2 point)
        {
            return point.X;
        }

        public double GetY(Vector2 point)
        {
            return point.Y;
        }

        public Vector2 Add(Vector2 a, Vector2 b)
        {
            return Vector2.Add(a, b);
        }
    }
}