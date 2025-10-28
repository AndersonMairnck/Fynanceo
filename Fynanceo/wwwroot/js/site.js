// Funções globais do sistema Fynanceo

// Inicialização quando o documento estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    inicializarComponentes();
    configurarEventListeners();
});

function inicializarComponentes() {
    // Auto-dismiss alerts após 5 segundos
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    const popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

function configurarEventListeners() {
    // Prevenir envio duplo de forms
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitButton = this.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Processando...';

                // Reativar o botão após 5 segundos (caso haja erro)
                setTimeout(() => {
                    submitButton.disabled = false;
                    submitButton.innerHTML = submitButton.dataset.originalText || 'Salvar';
                }, 5000);
            }
        });
    });
}

// Função para formatar moeda
function formatarMoeda(valor) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(valor);
}

// Função para formatar CPF/CNPJ
function formatarCpfCnpj(valor) {
    valor = valor.replace(/\D/g, '');

    if (valor.length === 11) {
        return valor.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } else if (valor.length === 14) {
        return valor.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }

    return valor;
}

// Função para mostrar loading
function mostrarLoading(mensagem = 'Carregando...') {
    // Implementar overlay de loading se necessário
    console.log(mensagem);
}

// Função para validar email
function validarEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

// Exportar funções para uso global
window.Fynanceo = {
    formatarMoeda,
    formatarCpfCnpj,
    validarEmail,
    mostrarLoading
};