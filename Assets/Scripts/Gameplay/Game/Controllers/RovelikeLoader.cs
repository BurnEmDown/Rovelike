using UnityCoreKit.Runtime.Bootstrap;
using UnityEngine.SceneManagement;

namespace Gameplay.Game.Controllers
{
    public class RovelikeLoader : Loader
    {
        public override void OnInitComplete()
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}