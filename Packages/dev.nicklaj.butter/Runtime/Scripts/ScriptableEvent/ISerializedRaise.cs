
namespace Dev.Nicklaj.Butter
{
    public interface ISerializedRaise
    {
        public bool HasArgument { get; }

        /// <summary>
        /// This function gets called when the raise button in the inspector is raised. Do not call via code.
        /// </summary>
        /// <param name="arg0"></param>
        public void OnRaiseButtonSubmit(string arg0 = "", uint channel = 0);
    }
}