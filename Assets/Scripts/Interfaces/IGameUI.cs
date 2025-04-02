using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IGameUI
{
    public UniTask Show();
    public UniTask Hide();
}
