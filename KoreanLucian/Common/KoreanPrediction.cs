namespace KoreanCommon
{
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    public class KoreanPrediction
    {
        private readonly List<PredictionItem> predictionItems;

        public KoreanPrediction(Spell spell, KoreanPredictionTypes type = KoreanPredictionTypes.Slow)
        {
            predictionItems = new List<PredictionItem>();
            foreach (Obj_AI_Hero objAiHero in HeroManager.Enemies)
            {
                predictionItems.Add(new PredictionItem(objAiHero, spell, type));
            }
        }

        public Vector3 GetPrediction(Obj_AI_Hero target)
        {
            return predictionItems.First(x => x.Target == target).GetPrediction();
        }

        public void Cast(Obj_AI_Hero target)
        {
            PredictionItem predictionItem = predictionItems.First(x => x.Target == target);
            Vector3 castPosition = predictionItem.GetPrediction();

            if (predictionItem.PredictionSpell.IsReady() && predictionItem.PredictionSpell.IsInRange(castPosition)
                && !castPosition.IsWall())
            {
                predictionItem.PredictionSpell.Cast(castPosition);
            }
        }
    }
}