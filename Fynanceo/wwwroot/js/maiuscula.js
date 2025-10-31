document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-maiuscula]').forEach(input => {
        const tipo = input.dataset.maiuscula;

        input.addEventListener('input', function (e) {
            let valor = e.target.value;

            // Capitaliza a primeira letra de cada palavra
            if (tipo === 'titulo' && valor.length >= 3) {
                const palavras = valor.split(' ');
                const capitalizado = palavras
                    .map(p => p.length > 0 ? p[0].toUpperCase() + p.slice(1).toLowerCase() : '')
                    .join(' ');

                if (capitalizado !== valor) {
                    e.target.value = capitalizado;
                }
            }

            // Transforma tudo em maiúsculas
            else if (tipo === 'tudo') {
                const maiusculo = valor.toUpperCase();
                if (maiusculo !== valor) {
                    e.target.value = maiusculo;
                }
            }
        });
    });
});
