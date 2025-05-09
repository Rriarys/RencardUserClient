namespace RencardUserClient.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetUserId(this HttpContext ctx)
            => ctx.User?.FindFirst("sub")?.Value;
    }
}
