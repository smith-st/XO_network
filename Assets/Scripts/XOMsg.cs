using UnityEngine.Networking;

namespace XO.NetworkMsg{
	public abstract class BaseXOMsg : MessageBase {
		abstract public short id {get;}
	}

	public class StartGameMsg : BaseXOMsg {
		override public short id  {get{
			return MsgType.Highest + 1;
		}}
		public bool myTurn;
	}
	public class NewTurnMsg : BaseXOMsg {
		override public short id  {get{
			return MsgType.Highest + 2;
		}}
		public int capturedCell = -1;
		public bool myTurn;
	}

	public class StopGameMsg : BaseXOMsg {
		override public short id  {get{
				return MsgType.Highest + 3;
		}}

		public StopGameParam param;
	}

}