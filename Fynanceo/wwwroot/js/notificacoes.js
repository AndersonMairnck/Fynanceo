function mostrarMensagemAjax(texto, tipo = 'info', duracao = 4000) {
    const div = $('#mensagemAjax');

    // Remove classes antigas
    div.removeClass('d-none alert-success alert-danger alert-warning alert-info');

    // Define tipo de alerta
    const tipoClasse = {
        sucesso: 'alert-success',
        erro: 'alert-danger',
        aviso: 'alert-warning',
        info: 'alert-info'
    }[tipo.toLowerCase()] || 'alert-info';

    // Aplica classe e texto
    div.addClass('alert ' + tipoClasse)
        .html(`<i class="fas ${getIcon(tipo)} me-2"></i>${texto}`)
        .removeClass('d-none');

    // Oculta automaticamente
    setTimeout(() => div.addClass('d-none'), duracao);
}

// Ícones diferentes conforme tipo
function getIcon(tipo) {
    switch (tipo.toLowerCase()) {
        case 'sucesso': return 'fa-check-circle';
        case 'erro': return 'fa-exclamation-circle';
        case 'aviso': return 'fa-exclamation-triangle';
        default: return 'fa-info-circle';
    }
}
