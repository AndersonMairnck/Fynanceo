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

function aplicarMensagensJqueryValidatePtBr() {
    if (!window.jQuery) return false;
    if (!window.jQuery.validator) return false;
    if (!window.jQuery.validator.messages) return false;

    window.jQuery.extend(window.jQuery.validator.messages, {
        required: 'Este campo é obrigatório.',
        remote: 'Por favor, corrija este campo.',
        email: 'Por favor, forneça um endereço de email válido.',
        url: 'Por favor, forneça uma URL válida.',
        date: 'Por favor, forneça uma data válida.',
        dateISO: 'Por favor, forneça uma data válida (ISO).',
        number: 'Por favor, forneça um número válido.',
        digits: 'Por favor, forneça somente dígitos.',
        creditcard: 'Por favor, forneça um cartão de crédito válido.',
        equalTo: 'Por favor, forneça o mesmo valor novamente.',
        accept: 'Por favor, forneça um valor com uma extensão válida.',
        maxlength: window.jQuery.validator.format('Por favor, não forneça mais que {0} caracteres.'),
        minlength: window.jQuery.validator.format('Por favor, forneça ao menos {0} caracteres.'),
        rangelength: window.jQuery.validator.format('Por favor, forneça um valor entre {0} e {1} caracteres.'),
        range: window.jQuery.validator.format('Por favor, forneça um valor entre {0} e {1}.'),
        max: window.jQuery.validator.format('Por favor, forneça um valor menor ou igual a {0}.'),
        min: window.jQuery.validator.format('Por favor, forneça um valor maior ou igual a {0}.')
    });

    return true;
}

document.addEventListener('DOMContentLoaded', function () {
    if (aplicarMensagensJqueryValidatePtBr()) return;

    var tentativas = 0;
    var timer = setInterval(function () {
        tentativas++;
        if (aplicarMensagensJqueryValidatePtBr() || tentativas >= 20) {
            clearInterval(timer);
        }
    }, 250);
});