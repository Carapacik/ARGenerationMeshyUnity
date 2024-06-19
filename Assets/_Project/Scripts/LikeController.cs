using UnityEngine;
using UnityEngine.UI;

public class LikeController : MonoBehaviour
{
    public int LikesCount = 0;
    public Text TextObject;


    private void Update()
    {
        TextObject.text = LikesCount.ToString();
    }

    public void AddLike()
    {
        LikesCount = LikesCount + 1;
    }

    public void RemoveLike()
    {
        if (LikesCount == 0)
        {
            return;
        }
        LikesCount = LikesCount - 1;
    }
}
