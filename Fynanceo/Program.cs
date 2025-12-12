							
using Fynanceo.Configuracao;
using Fynanceo.Data;
using Fynanceo.Models;					  
using Fynanceo.Service;
using Fynanceo.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Requer autenticação globalmente em TODOS os controllers
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar DbContext					   
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity com mensagens em português
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
    options.SignIn.RequireConfirmedEmail = false; // Mudar para true se usar confirmação de email
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<MensagensIdentityPortugues>(); // Mensagens em português

// Configurar Cookie de Autenticação
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
});

// Configurar Políticas de Autorização
builder.Services.AddAuthorization(options =>
{
    // Política Administrativa - Apenas Administrador
    options.AddPolicy(Politicas.AcessoAdministrativo, policy =>
        policy.RequireRole(PerfisUsuario.Administrador));

    // Política Gerencial - Administrador ou Gerente
    options.AddPolicy(Politicas.AcessoGerencial, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

    // Política de Caixa - Administrador, Gerente ou Caixa
    options.AddPolicy(Politicas.AcessoCaixa, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Caixa));

    // Política de Cozinha - Administrador, Gerente ou Cozinha
    options.AddPolicy(Politicas.AcessoCozinha, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Cozinha));

    // Política de Delivery - Administrador, Gerente ou Entregador
    options.AddPolicy(Politicas.AcessoDelivery, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Entregador));

    // Política de Atendimento - Administrador, Gerente ou Atendente
    options.AddPolicy(Politicas.AcessoAtendimento, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Atendente));

    // Gerenciar Usuários - Apenas Administrador
    options.AddPolicy(Politicas.GerenciarUsuarios, policy =>
        policy.RequireRole(PerfisUsuario.Administrador));

    // Gerenciar Produtos - Administrador ou Gerente
    options.AddPolicy(Politicas.GerenciarProdutos, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

    // Gerenciar Estoque - Administrador ou Gerente
    options.AddPolicy(Politicas.GerenciarEstoque, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

    // Gerenciar Financeiro - Administrador ou Gerente
    options.AddPolicy(Politicas.GerenciarFinanceiro, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

    // Visualizar Relatórios - Administrador ou Gerente
    options.AddPolicy(Politicas.VisualizarRelatorios, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente));

    // Realizar Vendas - Administrador, Gerente, Caixa ou Atendente
    options.AddPolicy(Politicas.RealizarVendas, policy =>
        policy.RequireRole(PerfisUsuario.Administrador, PerfisUsuario.Gerente, PerfisUsuario.Caixa, PerfisUsuario.Atendente));
});

// Registrar serviços
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
// SEED DATA - Popular dados iniciais
// ========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
																								
    
    try
    {
        Console.WriteLine("\n========================================");
        Console.WriteLine("Inicializando dados do sistema...");
        Console.WriteLine("========================================\n");
							   
								  
								
	  

        // Executar SeedData
        await SeedData.InicializarAsync(services);
													   
		 
																	
		 
	 

        // OPCIONAL: Criar usuários de teste (apenas em desenvolvimento)
        // Descomente a linha abaixo se quiser criar usuários de teste
        // await SeedData.CriarUsuariosTesteAsync(services, criarUsuariosTeste: app.Environment.IsDevelopment());

        Console.WriteLine("\n========================================");
        Console.WriteLine("Inicialização concluída!");
        Console.WriteLine("========================================\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ ERRO ao inicializar dados: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}\n");
								  
							   
													  
									
								  
						
		  
        
        // Em produção, você pode querer logar isso de forma mais robusta
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao popular dados iniciais do banco");
																					 
		 
    }
}

// Configure the HTTP request pipeline.
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

// IMPORTANTE: Ordem correta - Authentication antes de Authorization
app.UseAuthentication();																		
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();