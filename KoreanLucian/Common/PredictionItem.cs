namespace KoreanCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    internal class PredictionItem
    {
        private readonly int limit;

        private readonly int limitBias;

        private readonly Queue<Vector3> predictionList;

        public readonly Spell PredictionSpell;

        public readonly Obj_AI_Hero Target;

        private bool targetImmovable;

        public PredictionItem(Obj_AI_Hero target, Spell predictionSpell, KoreanPredictionTypes type = KoreanPredictionTypes.Slow)
        {
            PredictionSpell = predictionSpell;
            Target = target;
            predictionList = new Queue<Vector3>(limit);
            targetImmovable = false;

            switch (type)
            {
                case KoreanPredictionTypes.Fast:
                    limit = 5;
                    limitBias = 15;
                    break;
                case KoreanPredictionTypes.Medium:
                    limit = 10;
                    limitBias = 10;
                    break;
                default:
                    limit = 20;
                    limitBias = 5;
                    break;
            }

            Game.OnUpdate += ProcessPrediction;
        }

        public Vector3 GetPrediction()
        {
            if (targetImmovable)
            {
                return Target.ServerPosition;
            }

            if (predictionList.Count < limit)
            {
                return Vector3.Zero;
            }

            float bias = 0f;
            int count = 0;
            Vector3 lastPrediction = Vector3.Zero;
            Vector3 average = Vector3.Zero;

            foreach (Vector3 prediction in predictionList)
            {
                if (average == Vector3.Zero)
                {
                    lastPrediction = prediction;
                    average = prediction;
                }
                else
                {
                    average.X += prediction.X;
                    average.Y += prediction.Y;
                    average.X /= 2;
                    average.Y /= 2;
                    bias += lastPrediction.Distance(average);
                    count++;
                    lastPrediction = prediction;
                }
            }

            bias /= count;
            if (bias > limitBias || average.Distance(Target.Position) > PredictionSpell.Width * 2.5f)
            {
                return Vector3.Zero;
            }
            return average;
        }

        private void AddImmovable()
        {
            predictionList.Clear();
            targetImmovable = true;
        }

        private void AddPrediction(Vector3 position)
        {
            if (predictionList.Count == limit)
            {
                predictionList.Dequeue();
            }

            targetImmovable = false;
            predictionList.Enqueue(position);
        }

        private void ProcessPrediction(EventArgs args)
        {
            if (!Target.IsVisible && Target.IsDead
                && Target.Distance(ObjectManager.Player.Position) > PredictionSpell.Range + 150f)
            {
                predictionList.Clear();
                return;
            }

            if (Target.IsStunned || Target.IsWindingUp || Target.IsImmovable || Target.IsRooted || Target.IsZombie)
            {
                AddImmovable();
            }
            else if (!Target.IsMoving)
            {
                AddPrediction(Target.Position);
            }
            else
            {
                Vector3 predictedPosiction = Target.Path.LastOrDefault();

                for (int scape = 0; scape < 10; scape++)
                {
                    predictedPosiction.X += Target.Position.X;
                    predictedPosiction.Y += Target.Position.Y;

                    predictedPosiction.X /= 2;
                    predictedPosiction.Y /= 2;

                    if (Target.Distance(predictedPosiction)
                        < (Target.MoveSpeed * (PredictionSpell.Delay + 0.4f))
                        + ObjectManager.Player.Distance(predictedPosiction) / PredictionSpell.Speed)
                    {
                        break;
                    }
                }

                AddPrediction(predictedPosiction);
            }
        }
    }
}