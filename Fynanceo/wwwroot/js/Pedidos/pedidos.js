
        // ------------------------------
        // Filtro de pedidos por status
        // ------------------------------
        function filterPedidos(status) {

        const rows = document.querySelectorAll('.pedido-row');
        const searchTerm = document.getElementById('searchInput').value.toLowerCase();

        rows.forEach(row => {
        const rowStatus = row.getAttribute('data-status');
        const numeroPedido = row.getAttribute('data-numero').toLowerCase();

        const matchesStatus = status === 'all' || rowStatus === status;
        const matchesSearch = !searchTerm || numeroPedido.includes(searchTerm);

        row.style.display = (matchesStatus && matchesSearch) ? '' : 'none';
    });
    }

        // ------------------------------
        // Busca por número do pedido
        // ------------------------------
        function setupSearch() {

        const searchInput = document.getElementById('searchInput');

        searchInput.addEventListener('input', function() {

        const term = this.value.toLowerCase().trim();
        const rows = document.querySelectorAll('.pedido-row');

        rows.forEach(row => {
        const numero = row.getAttribute('data-numero').toLowerCase();

        row.style.display = (!term || numero.includes(term)) ? '' : 'none';
    });

        if (term) highlightSearchResults(term);
    });

        // Permite apertar Enter para aplicar o filtro
        searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') filterPedidos('all');
    });
    }

        // ------------------------------
        // Limpa o campo de busca
        // ------------------------------
        function clearSearch() {
        document.getElementById('searchInput').value = '';
        filterPedidos('all');
    }

        // ------------------------------
        // Destaca itens encontrados
        // ------------------------------
        function highlightSearchResults(term) {

        const rows = document.querySelectorAll('.pedido-row');

        rows.forEach(row => {
        const numeroCell = row.querySelector('td:first-child strong');
        const numero = row.getAttribute('data-numero');

        if (numero.toLowerCase().includes(term)) {
        row.classList.add('table-active');

        // Destaca trecho encontrado
        numeroCell.innerHTML =
        numero.replace(
        new RegExp(term, 'gi'),
        m => `<span class="bg-warning text-dark">${m}</span>`
        );
    } else {
        row.classList.remove('table-active');
    }
    });
    }


        
        
        
        // veio do detalhes pedido
        // wwwroot/js/pedido-details.js

        function atualizarStatus(novoStatus) {
            const pedidoId = document.querySelector('[data-pedido-id]').dataset.pedidoId;
            const url = `/Pedidos/AtualizarStatus/${pedidoId}`;

            if (!confirm(`Tem certeza que deseja alterar o status para "${novoStatus}"?`)) {
                return;
            }

            $.post(url, { novoStatus: novoStatus })
                .done(function(response) {
                    if (response.success) {
                        location.reload();
                    } else {
                        alert('Erro: ' + (response.message || 'Ocorreu um erro desconhecido.'));
                    }
                })
                .fail(function() {
                    alert('Erro ao atualizar status. Verifique sua conexão ou o servidor.');
                });
        }
        function enviaParaEntrega(id, numeroPedido, novoStatus) {
            // 1. A URL leva apenas o ID (para identificar QUEM é o pedido)
            const url = `/Pedidos/EnviaParaEntrega/${id}`;

            if (!confirm(`Confira se realmente todos os produtos do Pedido "${numeroPedido}" estão prontos?`)) {
                return;
            }

            // 2. O novoStatus vai como OBJETO no corpo da requisição
            // { nome_parametro_no_csharp : valor_variavel_js }
            $.post(url, { novoStatus: novoStatus })
                .done(function(response) {
                    if (response.success) {
                        location.reload();
                    } else {
                        alert('Erro: ' + (response.message || 'Erro desconhecido.'));
                    }
                })
                .fail(function(xhr, status, error) {
                    console.error(error);
                    alert('Erro ao conectar com o servidor.');
                });
        }

        // NO ~/js/Pedidos/Pedidos.js - ADICIONAR FUNÇÃO PARA ENVIAR PARA ENTREGA

        async function enviaParaEntrega(pedidoId, numeroPedido, status) {
            if (!confirm(`Confirmar envio do pedido ${numeroPedido} para entrega?`)) {
                return;
            }

            try {
                const response = await fetch(`/Pedidos/EnviaParaEntrega?Id=${pedidoId}&novoStatus=${status}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                });

                const result = await response.json();

                if (result.success) {
                    showAlert('success', result.message);
                    setTimeout(() => {
                        location.reload();
                    }, 1500);
                } else {
                    showAlert('error', result.message);
                }
            } catch (error) {
                showAlert('error', 'Erro ao processar solicitação: ' + error.message);
            }
        }

        // // Inicializa busca ao carregar a página
        // document.addEventListener('DOMContentLoaded', setupSearch);
        //
        // // Atualiza automaticamente a cada 30 segundos
        // setInterval(() => window.location.reload(), 30000);