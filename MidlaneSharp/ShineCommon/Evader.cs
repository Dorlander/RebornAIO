using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;
using Geometry = SPrediction.Geometry;
using SharpDX;

namespace ShineCommon
{
    public struct EvadeData
    {
        public Vector2 Position;
        public bool IsTargetted;
        public bool IsSelfCast;
        public Obj_AI_Base Target;

        public EvadeData(Vector2 v, bool bl1, bool bl2, Obj_AI_Base obj)
        {
            Position = v;
            IsTargetted = bl1;
            IsSelfCast = bl2;
            Target = obj;
        }
    }

    public class Evader
    {
        private EvadeMethods SpecialMethod;
        private Spell EvadeSpell;
        private Menu evade = null, shieldAlly = null;
        private Thread m_evade_thread;

        public ObjectPool<DetectedSpellData> m_spell_pool = new ObjectPool<DetectedSpellData>(() => new DetectedSpellData());
        public ConcurrentQueue<DetectedSpellData> m_spell_queue = new ConcurrentQueue<DetectedSpellData>();
        public ConcurrentQueue<EvadeData> m_evade_queue = new ConcurrentQueue<EvadeData>();

        public Evader(out Menu _evade, EvadeMethods method = EvadeMethods.None, Spell spl = null)
        {
            SpecialMethod = method;
            EvadeSpell = spl;
            evade = new Menu("Evade", "Evade");

            foreach (var enemy in HeroManager.Enemies)
            {
                foreach (var spell in SpellDatabase.EvadeableSpells.Where(p => p.ChampionName == enemy.ChampionName && p.EvadeMethods.HasFlag(method)))
                    evade.AddItem(new MenuItem(spell.SpellName, String.Format("{0} ({1})", spell.ChampionName, spell.Slot)).SetValue(true));
            }

            evade.AddItem(new MenuItem("EVADEMETHOD", "Evade Method: ").SetValue(new StringList(new[] { "Near Turret", "Less Enemies", "Auto" }, 2)));
            evade.AddItem(new MenuItem("EVADEENABLE", "Enabled").SetValue(false));

            if (method.HasFlag(EvadeMethods.MorganaE) || method.HasFlag(EvadeMethods.KayleR))
            {
                shieldAlly = new Menu("Ally Shielding", "allyshield");
                foreach (var ally in HeroManager.Allies)
                    if(!ally.IsMe)
                        shieldAlly.AddItem(new MenuItem("shield" + ally.ChampionName, "Shield " + ally.ChampionName).SetValue(true));

                shieldAlly.AddItem(new MenuItem("SHIELDENABLED", "Enabled").SetValue(true));
                evade.AddSubMenu(shieldAlly);
            }
            
            _evade = evade;
            m_evade_thread = new Thread(new ThreadStart(EvadeThread));
            m_evade_thread.Start();
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            
            Game.PrintChat("<font color='#ff3232'>Shine#: </font><font color='#d4d4d4'>Evader loaded for champion {0} !</font>", ObjectManager.Player.ChampionName);
        }

        public void SetEvadeSpell(Spell spl)
        {
            EvadeSpell = spl;
            evade.Item("EVADEENABLE").SetValue(true);
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (evade != null && evade.Item("EVADEENABLE").GetValue<bool>() && sender.Type == GameObjectType.obj_AI_Hero)
            {
                if (sender.IsEnemy)
                {
                    Vector2 sender_pos = sender.ServerPosition.To2D();
                    var item = evade.Items.FirstOrDefault(q => q.Name == args.SData.Name);
                    if (item != null && item.GetValue<bool>())
                    {
                        var spell = SpellDatabase.EvadeableSpells.FirstOrDefault(p => p.SpellName == args.SData.Name);
                        if (spell != null)
                        {
                            if (spell.IsSkillshot)
                            {
                                DetectedSpellData dcspell = m_spell_pool.GetObject();
                                dcspell.Set(spell, sender_pos, args.End.To2D(), sender, args);
                                m_spell_queue.Enqueue(dcspell);
                            }
                        }
                    }

                    //to do: ally check
                    if (item == null && args.Target != null && args.Target.IsMe && args != null && args.SData != null && !args.SData.IsAutoAttack() && sender.IsChampion())
                    {
                        if (sender.GetSpellDamage(ObjectManager.Player, args.SData.Name) * 2 >= ObjectManager.Player.Health)
                            OnSpellHitDetected(sender_pos, ObjectManager.Player);
                    }
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || args == null)
                return;

            EvadeData edata;
            if (m_evade_queue.TryDequeue(out edata))
            {
                Console.WriteLine("try evade with data Targetted: {0}, SelfCast: {1}, TargetName: {2}", edata.IsTargetted, edata.IsSelfCast, edata.Target.Name);
                if (EvadeSpell.IsReady())
                {
                    if ((SpecialMethod == EvadeMethods.ZedW && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedShadowDash") || SpecialMethod != EvadeMethods.ZedW)
                    {
                        if (edata.IsSelfCast)
                            EvadeSpell.Cast();
                        else if (edata.IsTargetted && edata.Target != null)
                            EvadeSpell.Cast(edata.Target);
                        else
                            EvadeSpell.Cast(edata.Position);

                        if (SpecialMethod == EvadeMethods.ZedW)
                            EvadeSpell.CastWithDelay(100);
                    }
                }
            }
        }

        public void OnSpellHitDetected(Vector2 direction, Obj_AI_Base target)
        {
            EvadeData edata;

            Vector2 evade_direction = direction.Perpendicular();
            Vector2 evade_pos = ObjectManager.Player.ServerPosition.To2D() + evade_direction * EvadeSpell.Range;

            bool position_needed = !(SpecialMethod.HasFlag(EvadeMethods.MorganaE) || SpecialMethod.HasFlag(EvadeMethods.LissandraR) || SpecialMethod.HasFlag(EvadeMethods.KayleR) || SpecialMethod.HasFlag(EvadeMethods.SivirE) || SpecialMethod.HasFlag(EvadeMethods.NocturneW) || SpecialMethod.HasFlag(EvadeMethods.VladimirW));

            if (position_needed)
            {
                switch (evade.Item("EVADEMETHOD").GetValue<StringList>().SelectedIndex)
                {
                    case 0: //near turret
                        CorrectNearTurret(ref evade_pos, evade_direction);
                        break;

                    case 1: //less enemies
                        CorrectLessEnemies(ref evade_pos, evade_direction);
                        break;

                    case 2: //both
                        if (!CorrectLessEnemies(ref evade_pos, evade_direction))
                            CorrectNearTurret(ref evade_pos, evade_direction);
                        break;
                }
            }

            edata = new EvadeData
                (evade_pos, 
                SpecialMethod.HasFlag(EvadeMethods.MorganaE) || SpecialMethod.HasFlag(EvadeMethods.LissandraR) || SpecialMethod.HasFlag(EvadeMethods.KayleR), 
                SpecialMethod.HasFlag(EvadeMethods.SivirE) || SpecialMethod.HasFlag(EvadeMethods.NocturneW) || SpecialMethod.HasFlag(EvadeMethods.VladimirW), 
                target);

            m_evade_queue.Enqueue(edata);
        }

        public void EvadeThread()
        {
            //TO DO: collision check (minion, champ, yasuo wall)
            DetectedSpellData dcspell;
            while (true)
            {
                try
                {
                    if (m_spell_queue.TryDequeue(out dcspell))
                    {
                        Vector2 my_pos = ObjectManager.Player.ServerPosition.To2D();
                        Vector2 sender_pos = dcspell.StartPosition;
                        Vector2 end_pos = dcspell.EndPosition;
                        Vector2 direction = (end_pos - sender_pos).Normalized();
                        if (sender_pos.Distance(end_pos) > dcspell.Spell.Range)
                            end_pos = sender_pos + direction * dcspell.Spell.Range;

                        Geometry.Polygon my_hitbox = ClipperWrapper.DefineRectangle(my_pos - 60, my_pos + 60, 60);
                        Geometry.Polygon spell_hitbox = null;

                        if (dcspell.Spell.IsSkillshot)
                        {
                            if (dcspell.Spell.Type == SkillshotType.SkillshotLine)
                                spell_hitbox = ClipperWrapper.DefineRectangle(sender_pos, end_pos, dcspell.Spell.Radius);
                            else if (dcspell.Spell.Type == SkillshotType.SkillshotCircle)
                                spell_hitbox = ClipperWrapper.DefineCircle(end_pos, dcspell.Spell.Radius);
                            else if (dcspell.Spell.Type == SkillshotType.SkillshotCone)
                                spell_hitbox = ClipperWrapper.DefineSector(sender_pos, end_pos - sender_pos, dcspell.Spell.Radius * (float)Math.PI / 180, dcspell.Spell.Range);
                        }

                        //spells with arc
                        if (dcspell.Spell.IsArc)
                        {
                            float mul = (end_pos.Distance(sender_pos) / (dcspell.Spell.Range - 20.0f));
                            
                            spell_hitbox = new Geometry.Polygon(
                                ClipperWrapper.DefineArc(sender_pos - dcspell.Spell.ArcData.Pos, end_pos, dcspell.Spell.ArcData.Angle * mul, dcspell.Spell.ArcData.Width, dcspell.Spell.ArcData.Height * mul),
                                ClipperWrapper.DefineArc(sender_pos - dcspell.Spell.ArcData.Pos, end_pos, dcspell.Spell.ArcData.Angle * mul, dcspell.Spell.ArcData.Width, (dcspell.Spell.ArcData.Height + dcspell.Spell.ArcData.Radius) * mul),
                                spell_hitbox);
                        }

                        if (spell_hitbox != null)
                        {
                            if (ClipperWrapper.IsIntersects(ClipperWrapper.MakePaths(my_hitbox), ClipperWrapper.MakePaths(spell_hitbox)))
                                OnSpellHitDetected(direction, ObjectManager.Player);
                            else
                            {
                                if ((SpecialMethod.HasFlag(EvadeMethods.MorganaE) || SpecialMethod.HasFlag(EvadeMethods.KayleR)) && shieldAlly != null && shieldAlly.Item("SHIELDENABLED").GetValue<bool>())
                                {
                                    var allies = ObjectManager.Player.GetAlliesInRange(EvadeSpell.Range).Where(p => !p.IsMe && shieldAlly.Item("shield" + p.ChampionName).GetValue<bool>()).OrderByDescending(q => Utility.GetPriority(q.ChampionName));

                                    if (allies != null)
                                    {
                                        foreach (Obj_AI_Base ally in allies)
                                        {
                                            Vector2 ally_pos = ally.ServerPosition.To2D();
                                            Geometry.Polygon ally_hitbox = ClipperWrapper.DefineRectangle(ally_pos, ally_pos + 60, 60);
                                            if (ClipperWrapper.IsIntersects(ClipperWrapper.MakePaths(ally_hitbox), ClipperWrapper.MakePaths(spell_hitbox)))
                                            {
                                                OnSpellHitDetected(direction, ally);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        m_spell_pool.PutObject(dcspell);
                    }
                }
                catch
                {

                }
                Thread.Sleep(1);
            }
        }

        private bool CorrectNearTurret(ref Vector2 evade_pos, Vector2 direction)
        {
            var turret = ObjectManager.Get<Obj_AI_Turret>().Where(p => p.IsAlly).MinOrDefault(q => q.ServerPosition.Distance(ObjectManager.Player.ServerPosition));
            if (turret != null)
            {
                if (turret.ServerPosition.To2D().Distance(evade_pos) > turret.ServerPosition.To2D().Distance(ObjectManager.Player.ServerPosition.To2D() - direction * EvadeSpell.Range))
                {
                    evade_pos = ObjectManager.Player.Position.To2D() - direction * EvadeSpell.Range;
                    return true;
                }
            }

            return false;
        }

        private bool CorrectLessEnemies(ref Vector2 evade_pos, Vector2 direction)
        {
            if (HeroManager.Enemies.Count(p => p.ServerPosition.To2D().Distance(ObjectManager.Player.ServerPosition.To2D() + direction * EvadeSpell.Range) <= ObjectManager.Player.BasicAttack.CastRange) > HeroManager.Enemies.Count(p => p.ServerPosition.To2D().Distance(ObjectManager.Player.ServerPosition.To2D() - direction * EvadeSpell.Range) <= ObjectManager.Player.BasicAttack.CastRange))
            {
                    evade_pos = ObjectManager.Player.Position.To2D() - direction * EvadeSpell.Range;
                    return true;
            }

            return false;
        }
    }
}
