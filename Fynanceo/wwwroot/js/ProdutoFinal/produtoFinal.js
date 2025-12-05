// ===============================
// CÁLCULO DE MARGEM DE LUCRO
// ===============================
function calcularMargem() {
    const custo = parseFloat(document.getElementById('custoUnitario').value.replace(/\./g, '').replace(',', '.')) || 0;
    const venda = parseFloat(document.getElementById('valorVenda').value.replace(/\./g, '').replace(',', '.')) || 0;

    const margem = venda - custo;
    const percentual = custo > 0 ? ((margem / custo) * 100) : 0;

    document.getElementById('margemLucro').textContent =
        margem.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

    document.getElementById('percentualLucro').textContent =
        percentual.toFixed(2) + '%';

    const margemElement = document.getElementById('margemLucro');
    const percentualElement = document.getElementById('percentualLucro');

    if (margem >= 0) {
        margemElement.className = 'form-control-plaintext fw-bold text-success';
        percentualElement.className = 'form-control-plaintext fw-bold text-success';
    } else {
        margemElement.className = 'form-control-plaintext fw-bold text-danger';
        percentualElement.className = 'form-control-plaintext fw-bold text-danger';
    }
}





// ===============================
// EVENTOS
// ===============================
document.addEventListener('DOMContentLoaded', function () {
    atualizarEventosRemover();
    calcularMargem();

    document.getElementById('custoUnitario').addEventListener('input', calcularMargem);
    document.getElementById('valorVenda').addEventListener('input', calcularMargem);
});
