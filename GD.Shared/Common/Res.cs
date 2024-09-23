namespace GD.Shared.Common
{
    public class Res<T>
    {
        public bool IsSuccess { get; init; } = false;
        public T? Data { get; set; }
        public List<string> ErrorList { get; set; }

        public Res(T data, bool isSuccess = true)
        {
            Data = data;
            IsSuccess = isSuccess;
        }

        public Res(string errorText)
        {
            IsSuccess = false;
            ErrorList = new List<string>() { errorText };
        }

        public Res(IEnumerable<string> errorTexts)
        {
            IsSuccess = false;
            ErrorList = errorTexts.ToList();
        }

        public Res()
        {
            IsSuccess = false;
        }
    }
}
