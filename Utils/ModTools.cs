using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;

namespace StardewOutfitManager.Utils
    // Just some tools to make my life modding SDV easier.
{
    public class CustomModTools
    {
        // Drawing Utilities
        public class DrawCustom
        {
            // Draw the farmer rescaled to a particular size with the correct subsprite offsets (This is untested with movement or action)
            //  Example use at 2x scale: drawFarmerScaled(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(0, 0), Color.White, 2f, _displayFarmer);
            //  Note: Don't try to scale this to decimals or blinking gets weird. Stick to 1f, 2f, etc.
            public static void drawFarmerScaled(SpriteBatch b, FarmerSprite.AnimationFrame animationFrame, int currentFrame, Rectangle sourceRect, Vector2 position, Color overrideColor, float scale, Farmer who)
            {
                int facingDirection = who.facingDirection;
                float layerDepth = 0.8f;
                float rotation = 0;
                Vector2 origin = Vector2.Zero;

                AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions").Invoke(who.FarmerRenderer, new object[] { who });
                position = new Vector2((float)Math.Floor(position.X), (float)Math.Floor(position.Y));
                var rotationAdjustment = Vector2.Zero;
                var positionOffset = new Vector2(animationFrame.xOffset * 4 * scale, animationFrame.positionOffset * 4 * scale);
                var baseTexture = AccessTools.FieldRefAccess<FarmerRenderer, Texture2D>(Game1.player.FarmerRenderer, "baseTexture");
                var pantsTexture = FarmerRenderer.pantsTexture;

                // Draw Body
                if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming.Value)
                {
                    sourceRect.Height /= 2;
                    sourceRect.Height -= (int)who.yOffset / 4;
                    position.Y += 64f;
                }
                if (facingDirection == 3 || facingDirection == 1)
                {
                    facingDirection = ((!animationFrame.flip) ? 1 : 3);
                }
                b.Draw(baseTexture, position + origin + positionOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
                
                // Draw Blink Animation
                if (!FarmerRenderer.isDrawingForUI && (bool)who.swimming.Value)
                {
                    if (who.currentEyes != 0 && who.FacingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)))
                    {
                        b.Draw(baseTexture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(5, 16, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 5E-08f);
                        b.Draw(baseTexture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4 + 20 + ((who.FacingDirection == 1) ? 12 : ((who.FacingDirection == 3) ? 4 : 0)), FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 40), new Rectangle(264 + ((who.FacingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (who.FacingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.2E-07f);
                    }
                    // Come back to this
                    drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth, positionOffset);
                    b.Draw(Game1.staminaRect, new Rectangle((int)position.X + (int)who.yOffset + 8, (int)position.Y - 128 + sourceRect.Height * 4 + (int)origin.Y - (int)who.yOffset, sourceRect.Width * 4 - (int)who.yOffset * 2 - 16, 4), Game1.staminaRect.Bounds, Color.White * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, layerDepth + 0.001f);
                    return;
                }

                // Draw Pants
                Rectangle pants_rect = new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
                // ints replace ClampPants
                int pantsIndex = who.GetPantsIndex();
                int pantsValue = (pantsIndex > Clothing.GetMaxPantsValue() || pantsIndex < 0) ? 0 : pantsIndex;
                pants_rect.X += (pantsValue % 10 * 192);
                pants_rect.Y += (pantsValue / 10 * 688);
                if (!who.IsMale)
                {
                    pants_rect.X += 96;
                }
                b.Draw(FarmerRenderer.pantsTexture, position + origin + positionOffset, pants_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetPantsColor()) : overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + ((who.FarmerSprite.CurrentAnimationFrame.frame == 5) ? 0.00092f : 9.2E-08f));
                sourceRect.Offset(288, 0);
                if (who.currentEyes != 0 && facingDirection != 0 && (Game1.timeOfDay < 2600 || (who.isInBed.Value && who.timeWentToBed.Value != 0)) && ((!who.FarmerSprite.PauseForSingleAnimation && !who.UsingTool) || (who.UsingTool && who.CurrentTool is FishingRod)) && (!who.UsingTool || !(who.CurrentTool is FishingRod fishing_rod) || fishing_rod.isFishing))
                {
                    int x_adjustment = 5;
                    x_adjustment = (animationFrame.flip ? (x_adjustment - FarmerRenderer.featureXOffsetPerFrame[currentFrame]) : (x_adjustment + FarmerRenderer.featureXOffsetPerFrame[currentFrame]));
                    switch (facingDirection)
                    {
                        case 1:
                            x_adjustment += 3;
                            break;
                        case 3:
                            x_adjustment++;
                            break;
                    }
                    x_adjustment *= 4 * (int)scale;
                    b.Draw(baseTexture, position + origin + positionOffset + new Vector2(x_adjustment, (FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && who.FacingDirection != 2) ? 36 : 40) * scale)), new Rectangle(5, 16, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 5E-08f);
                    b.Draw(baseTexture, position + origin + positionOffset + new Vector2(x_adjustment, (FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.FacingDirection == 1 || who.FacingDirection == 3) ? 40 : 44) * scale)), new Rectangle(264 + ((facingDirection == 3) ? 4 : 0), 2 + (who.currentEyes - 1) * 2, (facingDirection == 2) ? 6 : 2, 2), overrideColor, 0f, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.2E-07f);
                }
                
                // Draw Hair and Accessories (Including Shirt)
                drawHairAndAccesories(b, facingDirection, who, position, origin, scale, currentFrame, rotation, overrideColor, layerDepth, positionOffset);
                
                // Draw Arms (untested for scaling in motion)
                float arm_layer_offset = 4.9E-05f;
                if (facingDirection == 0)
                {
                    arm_layer_offset = -1E-07f;
                }
                sourceRect.Offset(-288 + (animationFrame.secondaryArm ? 192 : 96), 0);
                b.Draw(baseTexture, position + origin + positionOffset + who.armOffset, sourceRect, overrideColor, rotation, origin, 4f * scale, animationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + arm_layer_offset);
                if (!who.usingSlingshot || !(who.CurrentTool is Slingshot))
                {
                    return;
                }
                Slingshot slingshot = who.CurrentTool as Slingshot;
                Point point = Utility.Vector2ToPoint(slingshot.AdjustForHeight(Utility.PointToVector2(slingshot.aimPos.Value)));
                int mouseX = point.X;
                int y = point.Y;
                int backArmDistance = slingshot.GetBackArmDistance(who);
                Vector2 shoot_origin = slingshot.GetShootOrigin(who);
                float frontArmRotation = (float)Math.Atan2((float)y - shoot_origin.Y, (float)mouseX - shoot_origin.X) + (float)Math.PI;
                if (!Game1.options.useLegacySlingshotFiring)
                {
                    frontArmRotation -= (float)Math.PI;
                    if (frontArmRotation < 0f)
                    {
                        frontArmRotation += (float)Math.PI * 2f;
                    }
                }
                switch (facingDirection)
                {
                    case 0:
                        b.Draw(baseTexture, position + new Vector2(4f + frontArmRotation * 8f, -44f), new Rectangle(173, 238, 9, 14), Color.White, 0f, new Vector2(4f, 11f), 4f * scale, SpriteEffects.None, layerDepth + ((facingDirection != 0) ? 5.9E-05f : (-0.0005f)));
                        break;
                    case 1:
                        {
                            b.Draw(baseTexture, position + new Vector2(52 - backArmDistance, -32f), new Rectangle(147, 237, 10, 4), Color.White, 0f, new Vector2(8f, 3f), 4f * scale, SpriteEffects.None, layerDepth + ((facingDirection != 0) ? 5.9E-05f : 0f));
                            b.Draw(baseTexture, position + new Vector2(36f, -44f), new Rectangle(156, 244, 9, 10), Color.White, frontArmRotation, new Vector2(0f, 3f), 4f * scale, SpriteEffects.None, layerDepth + ((facingDirection != 0) ? 1E-08f : 0f));
                            int slingshotAttachX = (int)(Math.Cos(frontArmRotation + (float)Math.PI / 2f) * (double)(20 - backArmDistance - 8) - Math.Sin(frontArmRotation + (float)Math.PI / 2f) * -68.0);
                            int slingshotAttachY = (int)(Math.Sin(frontArmRotation + (float)Math.PI / 2f) * (double)(20 - backArmDistance - 8) + Math.Cos(frontArmRotation + (float)Math.PI / 2f) * -68.0);
                            Utility.drawLineWithScreenCoordinates((int)(position.X + 52f - (float)backArmDistance), (int)(position.Y - 32f - 4f), (int)(position.X + 32f + (float)(slingshotAttachX / 2)), (int)(position.Y - 32f - 12f + (float)(slingshotAttachY / 2)), b, Color.White);
                            break;
                        }
                    case 3:
                        {
                            b.Draw(baseTexture, position + new Vector2(40 + backArmDistance, -32f), new Rectangle(147, 237, 10, 4), Color.White, 0f, new Vector2(9f, 4f), 4f * scale, SpriteEffects.FlipHorizontally, layerDepth + ((facingDirection != 0) ? 5.9E-05f : 0f));
                            b.Draw(baseTexture, position + new Vector2(24f, -40f), new Rectangle(156, 244, 9, 10), Color.White, frontArmRotation + (float)Math.PI, new Vector2(8f, 3f), 4f * scale, SpriteEffects.FlipHorizontally, layerDepth + ((facingDirection != 0) ? 1E-08f : 0f));
                            int slingshotAttachX = (int)(Math.Cos(frontArmRotation + (float)Math.PI * 2f / 5f) * (double)(20 + backArmDistance - 8) - Math.Sin(frontArmRotation + (float)Math.PI * 2f / 5f) * -68.0);
                            int slingshotAttachY = (int)(Math.Sin(frontArmRotation + (float)Math.PI * 2f / 5f) * (double)(20 + backArmDistance - 8) + Math.Cos(frontArmRotation + (float)Math.PI * 2f / 5f) * -68.0);
                            Utility.drawLineWithScreenCoordinates((int)(position.X + 4f + (float)backArmDistance), (int)(position.Y - 32f - 8f), (int)(position.X + 26f + (float)slingshotAttachX * 4f / 10f), (int)(position.Y - 32f - 8f + (float)slingshotAttachY * 4f / 10f), b, Color.White);
                            break;
                        }
                    case 2:
                        b.Draw(baseTexture, position + new Vector2(4f, -32 - backArmDistance / 2), new Rectangle(148, 244, 4, 4), Color.White, 0f, Vector2.Zero, 4f * scale, SpriteEffects.None, layerDepth + ((facingDirection != 0) ? 5.9E-05f : 0f));
                        Utility.drawLineWithScreenCoordinates((int)(position.X + 16f), (int)(position.Y - 28f - (float)(backArmDistance / 2)), (int)(position.X + 44f - frontArmRotation * 10f), (int)(position.Y - 16f - 8f), b, Color.White);
                        Utility.drawLineWithScreenCoordinates((int)(position.X + 16f), (int)(position.Y - 28f - (float)(backArmDistance / 2)), (int)(position.X + 56f - frontArmRotation * 10f), (int)(position.Y - 16f - 8f), b, Color.White);
                        b.Draw(baseTexture, position + new Vector2(44f - frontArmRotation * 10f, -16f), new Rectangle(167, 235, 7, 9), Color.White, 0f, new Vector2(3f, 5f), 4f * scale, SpriteEffects.None, layerDepth + ((facingDirection != 0) ? 5.9E-05f : 0f));
                        break;
                    
                }
            }

            public static void drawHairAndAccesories(SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth, Vector2 positionOffset)
            {
                int heightOffset = who.IsMale == true ? 0 : 4;
                int hair_style = who.getHair();
                HairStyleMetadata hair_metadata = Farmer.GetHairStyleMetadata(who.hair.Value);
                var rotationAdjustment = Vector2.Zero;
                if (who != null && who.hat.Value != null && who.hat.Value.hairDrawType.Value == 1 && hair_metadata != null && hair_metadata.coveredIndex != -1)
                {
                    hair_style = hair_metadata.coveredIndex;
                    hair_metadata = Farmer.GetHairStyleMetadata(hair_style);
                }
                AccessTools.Method(typeof(FarmerRenderer), "executeRecolorActions").Invoke(who.FarmerRenderer, new object[] { who });
                // ints replace ClampShirt
                int shirtIndex = who.GetShirtIndex();
                int shirtValue = (shirtIndex > Clothing.GetMaxShirtValue() || shirtIndex < 0) ? 0 : shirtIndex;
                Rectangle shirtSourceRect = new Rectangle(shirtValue * 8 % 128, shirtValue * 8 / 128 * 32, 8, 8);
                Texture2D hair_texture = FarmerRenderer.hairStylesTexture;
                Rectangle hairstyleSourceRect = new Rectangle(hair_style * 16 % FarmerRenderer.hairStylesTexture.Width, hair_style * 16 / FarmerRenderer.hairStylesTexture.Width * 96, 16, 32);
                if (hair_metadata != null)
                {
                    hair_texture = hair_metadata.texture;
                    hairstyleSourceRect = new Rectangle(hair_metadata.tileX * 16, hair_metadata.tileY * 16, 16, 32);
                }
                Rectangle accessorySourceRect = ((int)who.accessory.Value >= 0) ? accessorySourceRect = new Rectangle((int)who.accessory.Value * 16 % FarmerRenderer.accessoriesTexture.Width, (int)who.accessory.Value * 16 / FarmerRenderer.accessoriesTexture.Width * 32, 16, 16) : new Rectangle();
                Rectangle hatSourceRect = (who.hat.Value != null) ? hatSourceRect = new Rectangle(20 * (int)who.hat.Value.which.Value % FarmerRenderer.hatsTexture.Width, 20 * (int)who.hat.Value.which.Value / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20) : new Rectangle();
                Rectangle dyed_shirt_source_rect = shirtSourceRect;
                float dye_layer_offset = 1E-07f;
                float hair_draw_layer = 2.2E-05f;
                
                // Added scaling at the correct positions
                switch (facingDirection)
                {
                    case 0:
                        shirtSourceRect.Offset(0, 24);
                        hairstyleSourceRect.Offset(0, 64);
                        dyed_shirt_source_rect = shirtSourceRect;
                        dyed_shirt_source_rect.Offset(128, 0);
                        if (who.hat.Value != null)
                        {
                            hatSourceRect.Offset(0, 60);
                        }
                        if (!who.bathingClothes.Value)
                        {
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16f + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset) * scale, shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f);
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2(16f + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), (float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset) * scale, dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.8E-07f + dye_layer_offset);
                        }
                        b.Draw(hair_texture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + 4 + ((who.IsMale && hair_style >= 16) ? (-4) : ((!who.IsMale && hair_style < 16) ? 4 : 0))) * scale, hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor.Value) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer);
                        break;
                    case 1:
                        shirtSourceRect.Offset(0, 8);
                        hairstyleSourceRect.Offset(0, 32);
                        dyed_shirt_source_rect = shirtSourceRect;
                        dyed_shirt_source_rect.Offset(128, 0);
                        if ((int)who.accessory.Value >= 0)
                        {
                            accessorySourceRect.Offset(0, 16);
                        }
                        if (who.hat.Value != null)
                        {
                            hatSourceRect.Offset(0, 20);
                        }
                        if (rotation == -(float)Math.PI / 32f)
                        {
                            rotationAdjustment.X = 6f;
                            rotationAdjustment.Y = -2f;
                        }
                        else if (rotation == (float)Math.PI / 32f)
                        {
                            rotationAdjustment.X = -6f;
                            rotationAdjustment.Y = 1f;
                        }
                        if (!who.bathingClothes.Value)
                        {
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16f + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), 56f + (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset) * scale, shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.8E-07f);
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16f + (float)(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), 56f + (float)(FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset) * scale, dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.8E-07f + dye_layer_offset);
                        }
                        if ((int)who.accessory.Value >= 0)
                        {
                            b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset) * scale, accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory.Value < 6) ? ((Color)who.hairstyleColor.Value) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + (((int)who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
                        }
                        b.Draw(hair_texture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair.Value >= 16) ? (-4) : ((!who.IsMale && (int)who.hair.Value < 16) ? 4 : 0))) * scale, hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor.Value) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer);
                        break;
                    case 2:
                        dyed_shirt_source_rect = shirtSourceRect;
                        dyed_shirt_source_rect.Offset(128, 0);
                        if (!who.bathingClothes.Value)
                        {
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2((16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), ((float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset - (float)(who.IsMale ? 0 : 0))) * scale, shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f);
                            b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + new Vector2((16 + FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4), ((float)(56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4) + (float)(int)heightOffset - (float)(who.IsMale ? 0 : 0))) * scale, dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + 1.5E-07f + dye_layer_offset);
                        }
                        if ((int)who.accessory.Value >= 0)
                        {
                            b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (8 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset - 4)) * scale, accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory.Value < 6) ? ((Color)who.hairstyleColor.Value) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + (((int)who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
                        }
                        b.Draw(hair_texture, position + origin + positionOffset + new Vector2(FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, (FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair.Value >= 16) ? (-4) : ((!who.IsMale && (int)who.hair.Value < 16) ? 4 : 0)))) * scale, hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor.Value) : overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + hair_draw_layer);
                        break;
                    case 3:
                        {
                            bool flip2 = true;
                            shirtSourceRect.Offset(0, 16);
                            dyed_shirt_source_rect = shirtSourceRect;
                            dyed_shirt_source_rect.Offset(128, 0);
                            if ((int)who.accessory.Value >= 0)
                            {
                                accessorySourceRect.Offset(0, 16);
                            }
                            if (hair_metadata != null && hair_metadata.usesUniqueLeftSprite)
                            {
                                flip2 = false;
                                hairstyleSourceRect.Offset(0, 96);
                            }
                            else
                            {
                                hairstyleSourceRect.Offset(0, 32);
                            }
                            if (who.hat.Value != null)
                            {
                                hatSourceRect.Offset(0, 40);
                            }
                            if (rotation == -(float)Math.PI / 32f)
                            {
                                rotationAdjustment.X = 6f;
                                rotationAdjustment.Y = -2f;
                            }
                            else if (rotation == (float)Math.PI / 32f)
                            {
                                rotationAdjustment.X = -5f;
                                rotationAdjustment.Y = 1f;
                            }
                            if (!who.bathingClothes.Value)
                            {
                                b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset) * scale, shirtSourceRect, overrideColor.Equals(Color.White) ? Color.White : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.5E-07f);
                                b.Draw(FarmerRenderer.shirtsTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(16 - FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 56 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset) * scale, dyed_shirt_source_rect, overrideColor.Equals(Color.White) ? Utility.MakeCompletelyOpaque(who.GetShirtColor()) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.None, layerDepth + 1.5E-07f + dye_layer_offset);
                            }
                            if ((int)who.accessory.Value >= 0)
                            {
                                b.Draw(FarmerRenderer.accessoriesTexture, position + origin + positionOffset + rotationAdjustment + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, 4 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + (int)heightOffset) * scale, accessorySourceRect, (overrideColor.Equals(Color.White) && (int)who.accessory.Value < 6) ? ((Color)who.hairstyleColor.Value) : overrideColor, rotation, origin, 4f * scale + ((rotation != 0f) ? 0f : 0f), SpriteEffects.FlipHorizontally, layerDepth + (((int)who.accessory.Value < 8) ? 1.9E-05f : 2.9E-05f));
                            }
                            b.Draw(hair_texture, position + origin + positionOffset + new Vector2(-FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((who.IsMale && (int)who.hair.Value >= 16) ? (-4) : ((!who.IsMale && (int)who.hair.Value < 16) ? 4 : 0))) * scale, hairstyleSourceRect, overrideColor.Equals(Color.White) ? ((Color)who.hairstyleColor.Value) : overrideColor, rotation, origin, 4f * scale, flip2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + hair_draw_layer);
                            break;
                        }
                }
                if (who.hat.Value != null && !who.bathingClothes.Value)
                {
                    bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
                    float layer_offset = 3.9E-05f;
                    if (who.hat.Value.isMask && facingDirection == 0)
                    {
                        Rectangle mask_draw_rect = hatSourceRect;
                        mask_draw_rect.Height -= 11;
                        mask_draw_rect.Y += 11;
                        b.Draw(FarmerRenderer.hatsTexture, position + origin + positionOffset + (new Vector2(0f, 44f) + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset.Value) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair.Value % 16] : 0) + 4 + (int)heightOffset)) * scale, mask_draw_rect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                        mask_draw_rect = hatSourceRect;
                        mask_draw_rect.Height = 11;
                        layer_offset = -1E-06f;
                        b.Draw(FarmerRenderer.hatsTexture, position + origin + positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset.Value) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair.Value % 16] : 0) + 4 + (int)heightOffset) * scale, mask_draw_rect, who.hat.Value.isPrismatic.Value ? Utility.GetPrismaticColor() : Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                    }
                    else
                    {
                        b.Draw(FarmerRenderer.hatsTexture, position + origin + positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset.Value) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair.Value % 16] : 0) + 4 + (int)heightOffset) * scale, hatSourceRect, who.hat.Value.isPrismatic.Value ? Utility.GetPrismaticColor() : Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                    }
                }
            }
        }
    }
}