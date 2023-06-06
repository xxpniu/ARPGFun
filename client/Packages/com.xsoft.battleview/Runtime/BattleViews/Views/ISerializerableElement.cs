using Google.Protobuf;

namespace BattleViews.Views
{
    //serialization
    public interface ISerializationElement
    {
        IMessage ToInitNotify();
    }
}
