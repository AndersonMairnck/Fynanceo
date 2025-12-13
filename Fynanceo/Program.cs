using Fynanceo.Configuracao;
using Fynanceo.Data;
using Fynanceo.Middleware;
using Fynanceo.Models;
using Fynanceo.Service;
using Fynanceo.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// ========================================
// Autenticação Global
// ========================================
builder.Services.AddControllersWithViews(options =>
{
																
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// ========================================
											  

// Configurar DbContext
// ========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========================================
// Configurar Sessão (ANTES do Identity)
// ========================================
builder.Services.AddDistributedMemoryCache(); // ✅ Cache para sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // ✅ Tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "Fynanceo.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ========================================
// Configurar Identity
// ========================================
builder.Services.AddIdentity<UsuarioAplicacao, IdentityRole>(options =>
{
    // Configurações de Senha
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 4;

    // Configurações de Bloqueio
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configurações de Usuário
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Configurações de Sign In
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<MensagensIdentityPortugues>();

// ========================================
// Configurar Cookie de Autenticação
// ========================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Usuario/Entrar";
    options.LogoutPath = "/Usuario/Sair";
    options.AccessDeniedPath = "/Usuario/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = "Fynanceo.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    
    // ✅ Importante: Eventos para controlar login/logout
    options.Events.OnSigningIn = context =>
    {
        // O cookie será criado após o SignIn
        return Task.CompletedTask;
    };
});

// ========================================
// Configurar Políticas de Autorização
// ========================================
builder.Services.AddAuthorization(options =>
{
													  
    options.AddPolicy(Politicas.AcessoAdministrativo, policy =>
        policy.RequireRole(PerfisUsuario.Administrador));

													 
    options.AddPolicy(Politicas.AcessoGerencial, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

														   
    options.AddPolicy(Politicas.AcessoCaixa, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Caixa));

															   
    options.AddPolicy(Politicas.AcessoCozinha, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Cozinha));

																   
    options.AddPolicy(Politicas.AcessoDelivery, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Entregador));

																	 
    options.AddPolicy(Politicas.AcessoAtendimento, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Atendente));

												 
    options.AddPolicy(Politicas.GerenciarUsuarios, policy =>
        policy.RequireRole(PerfisUsuario.Administrador));

													
    options.AddPolicy(Politicas.GerenciarProdutos, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

												   
    options.AddPolicy(Politicas.GerenciarEstoque, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

													  
    options.AddPolicy(Politicas.GerenciarFinanceiro, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

														
    options.AddPolicy(Politicas.VisualizarRelatorios, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

																   
    options.AddPolicy(Politicas.RealizarVendas, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Caixa, PerfisUsuario.Atendente));
									  
      // ✅ NOVAS: Políticas baseadas em Claims (permissões específicas)
    
    // Clientes
    options.AddPolicy("Permissao." + Permissoes.ClientesVisualizar, 
        policy => policy.RequireClaim("Permissao", Permissoes.ClientesVisualizar));
    options.AddPolicy("Permissao." + Permissoes.ClientesCriar, 
        policy => policy.RequireClaim("Permissao", Permissoes.ClientesCriar));
    options.AddPolicy("Permissao." + Permissoes.ClientesEditar, 
        policy => policy.RequireClaim("Permissao", Permissoes.ClientesEditar));
    options.AddPolicy("Permissao." + Permissoes.ClientesExcluir, 
        policy => policy.RequireClaim("Permissao", Permissoes.ClientesExcluir));
    
    // Produtos
    options.AddPolicy("Permissao." + Permissoes.ProdutosVisualizar, 
        policy => policy.RequireClaim("Permissao", Permissoes.ProdutosVisualizar));
    options.AddPolicy("Permissao." + Permissoes.ProdutosCriar, 
        policy => policy.RequireClaim("Permissao", Permissoes.ProdutosCriar));
    options.AddPolicy("Permissao." + Permissoes.ProdutosEditar, 
        policy => policy.RequireClaim("Permissao", Permissoes.ProdutosEditar));
    options.AddPolicy("Permissao." + Permissoes.ProdutosExcluir, 
        policy => policy.RequireClaim("Permissao", Permissoes.ProdutosExcluir));
    
    // Financeiro
    options.AddPolicy("Permissao." + Permissoes.FinanceiroVisualizar, 
        policy => policy.RequireClaim("Permissao", Permissoes.FinanceiroVisualizar));
    options.AddPolicy("Permissao." + Permissoes.FinanceiroEditar, 
        policy => policy.RequireClaim("Permissao", Permissoes.FinanceiroEditar));
    options.AddPolicy("Permissao." + Permissoes.FinanceiroAprovar, 
        policy => policy.RequireClaim("Permissao", Permissoes.FinanceiroAprovar));
    
    // Relatórios
    options.AddPolicy("Permissao." + Permissoes.RelatoriosVendas, 
        policy => policy.RequireClaim("Permissao", Permissoes.RelatoriosVendas));
    options.AddPolicy("Permissao." + Permissoes.RelatoriosFinanceiro, 
        policy => policy.RequireClaim("Permissao", Permissoes.RelatoriosFinanceiro));
    options.AddPolicy("Permissao." + Permissoes.RelatoriosEstoque, 
        policy => policy.RequireClaim("Permissao", Permissoes.RelatoriosEstoque));

    
});

// ========================================
// Registrar Serviços
// ========================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IMesaService, MesaService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IEntregaService, EntregaService>();
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();

builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IConfigService, ConfigService>();

builder.Services.AddMemoryCache();
							  


var app = builder.Build();
				 


													   


// ========================================
// Seed Data
// ========================================
															  
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    await SeedData.InicializarAsync(scope.ServiceProvider);
}

// ========================================
// Configure Pipeline
// ========================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ ORDEM CRÍTICA
app.UseSession();              // 1️⃣ Session primeiro
app.UseAuthentication();       // 2️⃣ Authentication
app.UseMiddleware<SessionValidationMiddleware>(); // 3️⃣ Validação de sessão customizada
app.UseAuthorization();        // 4️⃣ Authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();