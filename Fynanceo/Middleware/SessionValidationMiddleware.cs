using Fynanceo.Models;
using Microsoft.AspNetCore.Identity;

namespace Fynanceo.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<UsuarioAplicacao> userManager,
            SignInManager<UsuarioAplicacao> signInManager)
        {
            // ✅ Apenas validar se estiver autenticado
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    var user = await userManager.GetUserAsync(context.User);

                    if (user != null)
                    {
                        // ✅ Buscar SessionId do banco
                        var sessionIdInDb = user.CurrentSessionId;

                        // ✅ Buscar SessionId do cookie de sessão
                        var sessionIdInCookie = context.Session.GetString("SessionId");

                        // ✅ Se a sessão não bate, significa que fez login em outro lugar
                        if (!string.IsNullOrEmpty(sessionIdInDb) &&
                            !string.IsNullOrEmpty(sessionIdInCookie) &&
                            sessionIdInDb != sessionIdInCookie)
                        {
                            _logger.LogWarning(
                                "Sessão expirada para usuário {Email}. Login detectado em outro local.",
                                user.Email);

                            // ✅ Fazer logout
                            await signInManager.SignOutAsync();

                            // ✅ Limpar sessão
                            context.Session.Clear();

                            // ✅ Redirecionar para login com mensagem
                            context.Response.Redirect("/Usuario/Entrar?sessaoExpirada=true");
                            return;
                        }

                        // ✅ Se o SessionId não existe no cookie mas existe no DB, criar
                        if (string.IsNullOrEmpty(sessionIdInCookie) && !string.IsNullOrEmpty(sessionIdInDb))
                        {
                            context.Session.SetString("SessionId", sessionIdInDb);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao validar sessão do usuário");
                    // Em caso de erro, deixar continuar para não quebrar a aplicação
                }
            }

            await _next(context);
        }
    }
}