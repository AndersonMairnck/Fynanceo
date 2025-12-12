using Fynanceo.Configuracao;
using Fynanceo.Models;
using Microsoft.AspNetCore.Identity;

namespace Fynanceo.Data
{
    public static class SeedData
    {
        public static async Task InicializarAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<UsuarioAplicacao>>();

            await CriarPerfisAsync(roleManager);
            await CriarUsuarioAdministradorAsync(userManager);
            // OPCIONAL: Criar usuários de teste (apenas em desenvolvimento)
             //await SeedData.CriarUsuariosTesteAsync(serviceProvider, criarUsuariosTeste: true);
        }

        private static async Task CriarPerfisAsync(RoleManager<IdentityRole> roleManager)
        {
            // Lista de perfis do sistema
            string[] perfis = {
                PerfisUsuario.Administrador,
                PerfisUsuario.Gerente,
                PerfisUsuario.Caixa,
                PerfisUsuario.Cozinha,
                PerfisUsuario.Entregador,
                PerfisUsuario.Atendente
            };

            // Criar cada perfil se não existir
            foreach (var perfil in perfis)
            {
                if (!await roleManager.RoleExistsAsync(perfil))
                {
                    var resultado = await roleManager.CreateAsync(new IdentityRole(perfil));
                    
                    if (resultado.Succeeded)
                    {
                        Console.WriteLine($"✓ Perfil '{perfil}' criado com sucesso.");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Erro ao criar perfil '{perfil}':");
                        foreach (var erro in resultado.Errors)
                        {
                            Console.WriteLine($"  - {erro.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"→ Perfil '{perfil}' já existe.");
                }
            }
        }

        private static async Task CriarUsuarioAdministradorAsync(UserManager<UsuarioAplicacao> userManager)
        {
            // Dados do usuário administrador padrão
            var emailAdmin = "admin@fynanceo.com";
            var senhaAdmin = "Admin@123456";

            // Verificar se o usuário já existe
            var adminExistente = await userManager.FindByEmailAsync(emailAdmin);

            if (adminExistente == null)
            {
                // Criar novo usuário administrador
                var novoAdmin = new UsuarioAplicacao
                {
                    UserName = emailAdmin,
                    Email = emailAdmin,
                    NomeCompleto = "Administrador do Sistema",
                    Cargo = "Administrador",
                    EmailConfirmed = true,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow
                };

                // Criar usuário com senha
                var resultado = await userManager.CreateAsync(novoAdmin, senhaAdmin);

                if (resultado.Succeeded)
                {
                    // Adicionar ao perfil de Administrador
                    await userManager.AddToRoleAsync(novoAdmin, PerfisUsuario.Administrador);
                    
                    Console.WriteLine($"✓ Usuário administrador criado com sucesso!");
                    Console.WriteLine($"  E-mail: {emailAdmin}");
                    Console.WriteLine($"  Senha: {senhaAdmin}");
                    Console.WriteLine($"  IMPORTANTE: Altere a senha após o primeiro login!");
                }
                else
                {
                    Console.WriteLine($"✗ Erro ao criar usuário administrador:");
                    foreach (var erro in resultado.Errors)
                    {
                        Console.WriteLine($"  - {erro.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"→ Usuário administrador já existe: {emailAdmin}");
                
                // Verificar se está no perfil de administrador
                if (!await userManager.IsInRoleAsync(adminExistente, PerfisUsuario.Administrador))
                {
                    await userManager.AddToRoleAsync(adminExistente, PerfisUsuario.Administrador);
                    Console.WriteLine($"✓ Perfil 'Administrador' atribuído ao usuário existente.");
                }
            }
        }

        //Método opcional para criar usuários de teste (usar apenas em desenvolvimento)
        public static async Task CriarUsuariosTesteAsync(IServiceProvider serviceProvider, bool criarUsuariosTeste = false)
        {
            if (!criarUsuariosTeste) return;
        
            var userManager = serviceProvider.GetRequiredService<UserManager<UsuarioAplicacao>>();
        
            var usuariosTeste = new List<(string Nome, string Email, string Senha, string Cargo, string Perfil)>
            {
                ("João Silva", "gerente@fynanceo.com", "Gerente@123", "Gerente de Operações", PerfisUsuario.Gerente),
                ("Maria Santos", "caixa@fynanceo.com", "Caixa@123", "Operadora de Caixa", PerfisUsuario.Caixa),
                ("Pedro Costa", "cozinha@fynanceo.com", "Cozinha@123", "Chef de Cozinha", PerfisUsuario.Cozinha),
                ("Ana Oliveira", "atendente@fynanceo.com", "Atendente@123", "Atendente", PerfisUsuario.Atendente),
                ("Carlos Souza", "entregador@fynanceo.com", "Entregador@123", "Entregador", PerfisUsuario.Entregador)
            };
        
            Console.WriteLine("\n--- Criando Usuários de Teste ---");
        
            foreach (var (nome, email, senha, cargo, perfil) in usuariosTeste)
            {
                var usuarioExistente = await userManager.FindByEmailAsync(email);
        
                if (usuarioExistente == null)
                {
                    var novoUsuario = new UsuarioAplicacao
                    {
                        UserName = email,
                        Email = email,
                        NomeCompleto = nome,
                        Cargo = cargo,
                        EmailConfirmed = true,
                        Ativo = true,
                        DataCadastro = DateTime.UtcNow
                    };
        
                    var resultado = await userManager.CreateAsync(novoUsuario, senha);
        
                    if (resultado.Succeeded)
                    {
                        await userManager.AddToRoleAsync(novoUsuario, perfil);
                        Console.WriteLine($"✓ Usuário de teste criado: {email} (Perfil: {perfil})");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Erro ao criar usuário de teste {email}");
                    }
                }
                else
                {
                    Console.WriteLine($"→ Usuário de teste já existe: {email}");
                }
            }
        }
    }
}