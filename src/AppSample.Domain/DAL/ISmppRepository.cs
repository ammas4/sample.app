namespace AppSample.Domain.DAL
{
    public interface ISmppRepository
    {

        /// <param name="ctn">Без кода страны</param>
        /// <param name="text"></param>
        Task SendSmsInternalAsync(long ctn, string text);
    }
}
