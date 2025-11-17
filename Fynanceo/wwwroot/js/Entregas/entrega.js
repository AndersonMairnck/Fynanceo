// ===============================
// Atribuir Entregador
// ===============================

async function atribuirEntregador(entregaId) {
    $('#entregaId').val(entregaId);

    // Buscar entregadores disponíveis
    const response = await fetch('/Entregas/ObterEntregadoresDisponiveis');
    const entregadores = await response.json();

    const select = $('#entregadorSelect');
    select.empty().append('<option value="">Selecione um entregador...</option>');

    const lista = $('#entregadoresDisponiveis');
    lista.empty();

    if (entregadores.length === 0) {
        lista.html(`
            <div class="alert alert-warning">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Nenhum entregador disponível no momento.
            </div>
        `);
    } else {
        lista.html(`
            <h6>Entregadores Disponíveis:</h6>
            <div class="list-group">
                ${entregadores.map(e => `
                    <div class="list-group-item">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <strong>${e.nome}</strong><br>
                                <small class="text-muted">
                                    ${e.telefone} - ${e.tipoVeiculo}
                                    ${e.placa ? ` - ${e.placa}` : ''}
                                </small>
                            </div>
                            <button type="button"
                                    class="btn btn-sm btn-success"
                                    onclick="selecionarEntregador(${e.id}, '${e.nome}')">
                                <i class="fas fa-check"></i>
                            </button>
                        </div>
                    </div>
                `).join('')}
            </div>
        `);
    }

    $('#atribuirModal').modal('show');
}


// ===============================
// Selecionar entregador
// ===============================

async function selecionarEntregador(entregadorId, entregadorNome) {
    const entregaId = $('#entregaId').val();

    if (!entregaId) {
        alert('Erro: entrega não identificada.');
        return;
    }

    if (!confirm(`Deseja realmente atribuir esta entrega para ${entregadorNome}?`)) {
        return;
    }

    try {
        const response = await fetch(`/Entregas/AtribuirEntregador?id=${entregaId}&entregadorId=${entregadorId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (result.success) {
            $('#atribuirModal').modal('hide');
            location.reload();
        } else {
            alert('Erro: ' + result.message);
        }
    } catch (error) {
        alert('Falha ao atribuir entregador: ' + error.message);
    }
}


// ===============================
// Atualizar Status da Entrega
// ===============================

function atualizarStatus() {
    $('#statusSelect').val('');
    $('#observacaoStatus').val('');
    $('#statusModal').modal('show');
}

function marcarEmRota() {
    confirmarStatusAcao('EmRota', 'Marcar entrega como Em Rota?');
}

function marcarEntregue() {
    confirmarStatusAcao('Entregue', 'Marcar entrega como Entregue?');
}

function reportarProblema() {
    confirmarStatusAcao('Problema', 'Reportar problema na entrega?');
}

function confirmarStatusAcao(status, mensagem) {
    if (confirm(mensagem)) {
        $('#statusSelect').val(status);
        confirmarStatus();
    }
}


// ===============================
// Confirmação final do status
// ===============================

async function confirmarStatus() {
    const status = $('#statusSelect').val();
    const observacao = $('#observacaoStatus').val();
    const entregaId = $('#entregaIdHidden').val(); // OU ajuste para seu campo correto

    if (!status) {
        alert('Selecione um status');
        return;
    }

    try {
        const response = await fetch(`/Entregas/AtualizarStatus?id=${entregaId}&novoStatus=${status}&observacao=${encodeURIComponent(observacao)}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (result.success) {
            $('#statusModal').modal('hide');
            location.reload();
        } else {
            alert('Erro: ' + result.message);
        }
    } catch (error) {
        alert('Falha ao atualizar status: ' + error.message);
    }
}


// ===============================
// Filtrar entregas
// ===============================

    function filterEntregas() {
    const status = ($('#statusFilter').val() || '').toString().trim();
    const search = ($('#searchFilter').val() || '').toString().toLowerCase().trim();
    const entregador = ($('#entregadorFilter').val() || '').toString().trim();

    $('.entrega-row').each(function() {
    const row = $(this);

    const rowStatus = (row.data('status') || '').toString().trim();
    const rowPedido = (row.data('pedido') || '').toString().toLowerCase().trim();
    const rowEndereco = row.find('td').eq(2).text().toLowerCase().trim();
    const rowEntregador = (row.data('entregador') || '').toString().trim();

    const statusMatch = status === 'all' || rowStatus === status;
    const searchMatch = search === '' || rowPedido.includes(search) || rowEndereco.includes(search);
    const entregadorMatch = entregador === 'all' || rowEntregador === entregador;

    row.toggle(statusMatch && searchMatch && entregadorMatch);
});
}

    function resetFilters() {
    $('#statusFilter').val('all');
    $('#searchFilter').val('');
    $('#entregadorFilter').val('all');
    filterEntregas();
}
