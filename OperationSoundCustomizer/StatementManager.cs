using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OperationSoundCustomizer.Condition;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;

namespace OperationSoundCustomizer {
    public interface IInputAccesser {
        public InputCode UpdatedCode { get; }

        //boolかPointかfloat
        public T GetValue<T>(InputCode code);
    }


    //条件
    namespace Condition {
        public abstract record Condition {

            private static readonly List<Condition> conditions = new();
            public static void AllUpdate(IInputAccesser accesser) {
                foreach (var c in conditions) {
                    c.Update(accesser);
                }
            }


            public Condition() {
                conditions.Add(this);
            }

            [JsonIgnore]
            public virtual bool IsOK { protected set; get; }

            protected virtual void Update(IInputAccesser accesser) { }

            public static Condition operator |(Condition l, Condition r) {
                return new AnyCondition(new List<Condition> { l, r });
            }
            public static Condition operator &(Condition l, Condition r) {
                return new AllCondition(new List<Condition> { l, r });
            }
            public static Condition operator !(Condition l) {
                return new NotCondition(l);
            }
        }


        //論理演算
        public abstract record LogicalCondition : Condition {
        }
        //全部trueならtrue
        public record AllCondition(IEnumerable<Condition> Conditions) : LogicalCondition {
            public override bool IsOK => Conditions.All(c => c.IsOK);
        }
        //どれかtrueならtrue
        public record AnyCondition(IEnumerable< Condition> Conditions) : LogicalCondition {
            public override bool IsOK => Conditions.Any(c => c.IsOK);
        }

        //NOT
        public record NotCondition(Condition Condition) : LogicalCondition {
            public override bool IsOK => !Condition.IsOK;
        }


        //今どれかが押された
        public record WasAnyDownNow(Device Device) : Condition {
            protected override void Update(IInputAccesser accesser) {
                IsOK = accesser.UpdatedCode.GetInputType() == InputType.Button &&
                accesser.UpdatedCode.GetDevice() == Device &&
                accesser.GetValue<BoolSet>(accesser.UpdatedCode).WasDown;
            }
        }
        //今どれかが離された
        public record IsAnyReleasedNow(Device Device) : Condition {
            protected override void Update(IInputAccesser accesser) {
                IsOK = accesser.UpdatedCode.GetInputType() == InputType.Button &&
                accesser.UpdatedCode.GetDevice() == Device &&
                accesser.GetValue<BoolSet>(accesser.UpdatedCode).IsReleased;
            }
        }


        //1つのバーチャルキーに対する条件
        public abstract record CodeCondition(InputCode Code) : Condition;


        //とりあえず押されてる
        public record IsWown(InputCode Code) : CodeCondition(Code) {
            protected override void Update(IInputAccesser accesser) {
                IsOK = accesser.GetValue<bool>(Code);
            }
        }

        //今押された
        public record WasDownNow(InputCode Code) : CodeCondition(Code) {
            protected override void Update(IInputAccesser accesser) {
                IsOK = accesser.GetValue<BoolSet>(Code).WasDown;
            }
        }
        //今離された
        public record IsReleasedNow(InputCode Code) : CodeCondition(Code) {
            protected override void Update(IInputAccesser accesser) {
                IsOK = accesser.GetValue<BoolSet>(Code).IsReleased;
            }
        }



        public record AnyCondition<T>(IEnumerable<InputCode> Params) : Condition where T : CodeCondition {
            readonly CodeCondition[] codeConditions = Params.Select(key => Activator.CreateInstance(typeof(T), key) as CodeCondition).Where(C => C != null).ToArray();
            public override bool IsOK => codeConditions.Any(c => c.IsOK);
        }

        public record AllCondition<T>(IEnumerable<InputCode> Params) : Condition where T : CodeCondition {
            readonly CodeCondition[] codeConditions = Params.Select(key => Activator.CreateInstance(typeof(T), key) as CodeCondition).Where(C => C != null).ToArray();
            public override bool IsOK => codeConditions.All(c => c.IsOK);
        }


    }

    //条件と鳴らす音だから文？
    public interface IStatement {
        public bool Execute(IInputAccesser accesser);
    }

    //条件1個で音１個ならす
    public record SingleStatement(Condition.Condition Condition, IAudio Audio) : IStatement {
        public bool Execute(IInputAccesser accesser) {
            if (Condition.IsOK) {
                Audio.Start();
                return true;
            }
            return false;
        }
    }
    //条件1個でリストで音１個ならす
    public record SingleListStatement(Condition.Condition Condition, IValueList<IAudio> Audios) : IStatement {
        public bool Execute(IInputAccesser accesser) {
            if (Condition.IsOK) {
                Audios.GetNextValue().Start();
                return true;
            }
            return false;
        }
    }
    //条件2個で音のペアを鳴らす
    public record PairStatement(Condition.Condition DownCondition, Condition.Condition UpCondition, AudioPair AudioPair) : IStatement {
        public bool Execute(IInputAccesser accesser) {
            if (DownCondition.IsOK) {
                AudioPair.Start(true);
                return true;
            }
            if (UpCondition.IsOK) {
                AudioPair.Start(false);
                return true;
            }
            return false;
        }
    }
    //条件2個でリストで音のペアを鳴らす
    public record PairListStatement(Condition.Condition DownCondition, Condition.Condition UpCondition, LockValues<AudioPair> AudioPairs) : IStatement {
        public bool Execute(IInputAccesser accesser) {
            if (DownCondition.IsOK) {
                AudioPairs.LockValue((int)accesser.UpdatedCode).Start(true);
                return true;
            }
            if (UpCondition.IsOK) {
                if (AudioPairs.UnlockValue((int)accesser.UpdatedCode, out var audio)) {
                    audio.Start(false);
                    return true;
                }
            }
            return false;
        }
    }

    class StatementManager {
        private static readonly List<IStatement> statements = new();
        public static void ClearStatement() {
            statements.Clear();
        }
        public static void RegisterStatement(IStatement statement) {
            statements.Add(statement);
        }
        public static void ExecutingStatement(IInputAccesser accesser) {
            Condition.Condition.AllUpdate(accesser);

            foreach (var statement in statements) {
                if (statement.Execute(accesser)) {
                    break;
                }
            }
        }
    }
}
