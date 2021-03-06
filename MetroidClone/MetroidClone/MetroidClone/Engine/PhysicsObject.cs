﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetroidClone.Engine
{
    class PhysicsObject : GameObject
    {
        public Rectangle BoundingBox { get; protected set; }
        public Rectangle TranslatedBoundingBox
        {
            get
            {
                Rectangle translatedBoundingBox = BoundingBox;
                translatedBoundingBox.Offset(Position.ToPoint());
                return translatedBoundingBox;
            }
        }
        public bool CollideWithWalls = true;
        public Vector2 Speed = Vector2.Zero;
        Vector2 subPixelSpeed = Vector2.Zero;
        public Vector2 PositionPrevious = Vector2.Zero;
        protected float XFriction = 0.8f;
        protected float Gravity = 0.2f;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Speed.X *= XFriction;

            //resolve speeds
            Speed.Y += Gravity;
            PositionPrevious = Position;

            //check collision
            if (CollideWithWalls)
                MoveCheckingWallCollision();
            else
                Position += Speed;

        }

        public override void Draw()
        {
            //Draw the current image of the sprite. By default, the size of the bounding box is used.
            if (CurrentSprite != null && Visible)
                Drawing.DrawSprite(CurrentSprite, Position, (int)CurrentImage, ImageScaling * new Vector2(BoundingBox.Width, BoundingBox.Height));
        }

        void MoveCheckingWallCollision()
        {
            //subPixelSpeed saved for the next frame
            Point roundedSpeed;
            subPixelSpeed += Speed;
            roundedSpeed = new Point((int)Math.Round(Speed.X), (int)Math.Round(Speed.Y));
            subPixelSpeed -= roundedSpeed.ToVector2();

            //move for X until collision
            for (int i = 0; i < Math.Abs(roundedSpeed.X); i++)
            {
                if (InsideWall(Position.X + Math.Sign(roundedSpeed.X), Position.Y, BoundingBox))
                {
                    Speed.X = 0;
                    break;
                }
                else
                    Position.X += Math.Sign(roundedSpeed.X);
            }

            //move for Y until collision
            for (int i = 0; i < Math.Abs(roundedSpeed.Y); i++)
            {
                if (InsideWall(Position.X, Position.Y + Math.Sign(roundedSpeed.Y), BoundingBox))
                {
                    Speed.Y = 0;
                    break;
                }
                else
                    Position.Y += Math.Sign(roundedSpeed.Y);
            }
        }

        protected bool InsideWall(Rectangle boundingbox)
        {
            Level level = World.Level;

            Point gridPosition = (new Vector2(Position.X / level.TileSize.X, Position.Y / level.TileSize.Y)).ToPoint();
            Point min = new Point(gridPosition.X - 3, gridPosition.Y - 3);
            Point max = new Point(gridPosition.X + 3, gridPosition.Y + 3);

            min = min.ClampPoint(Point.Zero, level.LevelDimensions);
            max = max.ClampPoint(Point.Zero, level.LevelDimensions);

            for (int xp = min.X; xp < max.X; xp++)
                for (int yp = min.Y; yp < max.Y; yp++)
                    if (level.Grid[xp, yp])
                    {
                        Rectangle tile = new Rectangle(xp * level.TileSize.X, yp * level.TileSize.Y, level.TileSize.X, level.TileSize.Y);
                        if (boundingbox.Intersects(tile))
                            return true;
                    }

            return false;
        }

        protected bool InsideWall(Point position, Rectangle boundingbox)
        {
            Rectangle box = boundingbox;
            box.Offset(position);
            return InsideWall(box);
        }

        protected bool InsideWall(Vector2 position, Rectangle boundingbox)
        {
            return InsideWall(position.ToPoint(), boundingbox);
        }

        protected bool InsideWall(float x, float y, Rectangle boundingbox)
        {
            return InsideWall(new Point((int)x, (int)y), boundingbox);
        }
    }
}
