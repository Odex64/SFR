using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SFD;

namespace SFR.Bootstrap;

internal class AnimationHandler
{
    internal class AnimationFrame
    {
        internal List<AnimationPart> Parts;
        internal List<AnimationCollision> Collisions;

        internal string Event;
        internal int Time;

        internal AnimationFrame()
        {
            Parts = [];
            Collisions = [];
            Event = string.Empty;
            Time = 100;
        }
    }

    internal class AnimationPart
    {
        internal int Id;

        internal float X;
        internal float Y;

        internal float Rotation;
        internal SpriteEffects Flip;

        internal float ScaleX;
        internal float ScaleY;

        internal string PostFix;
    }

    internal class AnimationCollision
    {
        internal const int LEGS = 1;
        internal const int BODY = 2;
        internal const int HEAD = 3;

        internal int Id;

        internal float TLX;
        internal float TLY;

        internal float BRX;
        internal float BRY;
    }

    public static AnimationsData LoadAnimationsDataPipeline(string path) => Process(Import(path));

    public static string Import(string filename)
    {
        string fullPath = Path.GetFullPath(filename);
        string[] array = Directory.GetFiles(fullPath);
        string text = "|";
        for (int i = 0; i < array.Length; i++)
        {
            string text2 = array[i];
            text2 = text2.Substring(text2.LastIndexOf("\\") + 1);
            text2 = text2.Substring(0, text2.LastIndexOf('.'));
            if (text2 != "char_anims")
            {
                text = text + text2 + "=";
                text += File.ReadAllText(array[i]);
                text += '|';
            }
        }

        return text;
    }

    public static AnimationsData Process(string input)
    {
        string[] array = input.Split(['|']);
        AnimationData[] array2 = new AnimationData[array.Length - 2];
        for (int i = 1; i < array.Length - 1; i++)
        {
            string[] array3 = array[i].Split(['=']);
            string text = array3[0];
            string[] array4 = array3[1].Split(['\n']);
            TrimEnds(ref array4);
            List<AnimationFrame> list = [];
            for (int j = 0; j < array4.Length; j++)
            {
                if (array4[j] == "frame")
                {
                    AnimationFrame animationFrame = new();
                    list.Add(animationFrame);
                    j++;
                    bool flag = false;
                    while (j != array4.Length && !flag)
                    {
                        string[] array5 = array4[j].Split([' ']);
                        if (array5[0] == "part")
                        {
                            AnimationPart animationPart = new();
                            animationPart.Id = int.Parse(array5[1]);
                            animationPart.X = float.Parse(array5[2]);
                            animationPart.Y = float.Parse(array5[3]);
                            animationPart.Rotation = float.Parse(array5[4]);
                            animationPart.Flip = (SpriteEffects)int.Parse(array5[5]);
                            animationPart.ScaleX = float.Parse(array5[6]);
                            animationPart.ScaleY = float.Parse(array5[7]);
                            if (array5.Length > 8)
                            {
                                animationPart.PostFix = array5[8];
                            }
                            else
                            {
                                animationPart.PostFix = "";
                            }

                            animationFrame.Parts.Add(animationPart);
                        }
                        else if (array5[0] == "collision")
                        {
                            AnimationCollision animationCollision = new();
                            animationCollision.Id = int.Parse(array5[1]);
                            animationCollision.TLX = float.Parse(array5[2]);
                            animationCollision.TLY = float.Parse(array5[3]);
                            animationCollision.BRX = float.Parse(array5[4]);
                            animationCollision.BRY = float.Parse(array5[5]);
                            animationFrame.Collisions.Add(animationCollision);
                        }
                        else if (array5[0] == "time")
                        {
                            animationFrame.Time = int.Parse(array5[1]);
                        }
                        else if (array5[0] == "event")
                        {
                            animationFrame.Event = array5[1];
                        }

                        if (array5[0] != "frame")
                        {
                            j++;
                        }
                        else
                        {
                            flag = true;
                            j--;
                        }
                    }
                }
            }

            AnimationFrameData[] array6 = new AnimationFrameData[list.Count];
            for (int k = 0; k < array6.Length; k++)
            {
                AnimationPartData[] array7 = new AnimationPartData[list[k].Parts.Count];
                for (int l = 0; l < array7.Length; l++)
                {
                    array7[l] = new AnimationPartData(list[k].Parts[l].Id, list[k].Parts[l].X, list[k].Parts[l].Y, list[k].Parts[l].Rotation, list[k].Parts[l].Flip, list[k].Parts[l].ScaleX, list[k].Parts[l].ScaleY, list[k].Parts[l].PostFix);
                }

                AnimationCollisionData[] array8 = new AnimationCollisionData[list[k].Collisions.Count];
                for (int m = 0; m < array8.Length; m++)
                {
                    float num = list[k].Collisions[m].TLX - list[k].Collisions[m].BRX;
                    float num2 = list[k].Collisions[m].TLY - list[k].Collisions[m].BRY;
                    float num3 = list[k].Collisions[m].TLX + num / 2f;
                    float num4 = list[k].Collisions[m].TLY + num2 / 2f;
                    array8[
                        m] = new AnimationCollisionData(list[k].Collisions[m].Id, num3, num4, num, num2);
                }

                array6[k] = new AnimationFrameData(array7, array8, list[k].Event, list[k].Time);
            }

            array2[i - 1] = new AnimationData(array6, text);
        }

        return new AnimationsData(array2);
    }

    public static void TrimEnds(ref string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].EndsWith("\r"))
            {
                lines[i] = lines[i].Remove(lines[i].Length - 1);
            }
        }
    }
}