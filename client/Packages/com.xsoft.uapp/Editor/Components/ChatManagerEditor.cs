using UApp;
using UnityEditor;


[CustomEditor(typeof(ChatManager))]
public class ChatManagerEditor:Editor
{
    private bool _showFriends = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var chatManager = this.target as ChatManager;
        _showFriends = EditorGUILayout.ToggleLeft("Friend", _showFriends);
        if (_showFriends)
        {
            var friends = chatManager!.Friends;
            foreach (var (k,v) in friends)
            {
                EditorGUILayout.LabelField($"{v.User.UserName}",$"{v.State}");
            }
        }
    }

}