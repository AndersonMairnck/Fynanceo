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
// INGREDIENTES – ADD & REMOVE
// ===============================
let ingredienteIndex = document.querySelectorAll('.ingrediente-item').length;

function atualizarEventosRemover() {
    document.querySelectorAll('.btn-remover-ingrediente').forEach(btn => {
        btn.onclick = function () {
            const item = this.closest('.ingrediente-item');
            if (document.querySelectorAll('.ingrediente-item').length > 1) {
                item.remove();
            }
        };
    });
}

document.getElementById('btn-adicionar-ingrediente').addEventListener('click', function () {
    const container = document.getElementById('ingredientes-container');

    const div = document.createElement('div');
    div.className = 'ingrediente-item row mb-3';
    div.innerHTML = `
        <div class="col-md-4">
            <input name="Ingredientes[${ingredienteIndex}].Nome" 
                   class="form-control" 
                   placeholder="Nome do Material ou Produto" 
                   />
        </div>
        <div class="col-md-3">
            <input name="Ingredientes[${ingredienteIndex}].Quantidade" 
                   type="number" step="0.001" 
                   class="form-control" 
                   placeholder="Quantidade" />
        </div>
        <div class="col-md-3">
            <select name="Ingredientes[${ingredienteIndex}].UnidadeMedida" class="form-select">
                <option value="">Selecione</option>
                <option value="g">Gramas (g)</option>
                <option value="kg">Quilogramas (kg)</option>
                <option value="ml">Mililitros (ml)</option>
                <option value="L">Litros (L)</option>
                <option value="un">Unidades (un)</option>
                <option value="xíc">Xícaras</option>
                <option value="col">Colheres</option>
            </select>
        </div>
        <div class="col-md-2">
            <button type="button" class="btn btn-danger btn-remover-ingrediente">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    `;

    container.appendChild(div);
    ingredienteIndex++;

    atualizarEventosRemover();
    // aplicarMaiuscula();
});




// ===============================
// EVENTOS
// ===============================
document.addEventListener('DOMContentLoaded', function () {
    atualizarEventosRemover();
    calcularMargem();

    document.getElementById('custoUnitario').addEventListener('input', calcularMargem);
    document.getElementById('valorVenda').addEventListener('input', calcularMargem);
});
