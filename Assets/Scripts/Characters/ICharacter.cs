using System.Collections;

namespace Characters
{
    public interface ICharacter
    {
        void Hit(string colliderName);
        IEnumerator DieRoutine();
        void UpdateCustomBehaviour();
    }
}
