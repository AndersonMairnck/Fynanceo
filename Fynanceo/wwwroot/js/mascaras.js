document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-mask]').forEach(input => {
        const tipo = input.dataset.mask;

        input.addEventListener('input', function (e) {
            let v = e.target.value.replace(/\D/g, ''); // remove tudo que não é número

            if (tipo === 'cpfcnpj') {
                // Limita a 14 dígitos
                if (v.length > 14) v = v.slice(0, 14);

                // Se for até 11 → CPF | Se for acima → CNPJ
                if (v.length <= 11) {
                    // CPF: 000.000.000-00
                    if (v.length > 9)
                        v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
                    else if (v.length > 6)
                        v = v.replace(/(\d{3})(\d{3})(\d{0,3})/, '$1.$2.$3');
                    else if (v.length > 3)
                        v = v.replace(/(\d{3})(\d{0,3})/, '$1.$2');
                } else {
                    // CNPJ: 00.000.000/0000-00
                    if (v.length > 12)
                        v = v.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{0,2})/, '$1.$2.$3/$4-$5');
                    else if (v.length > 8)
                        v = v.replace(/(\d{2})(\d{3})(\d{3})(\d{0,4})/, '$1.$2.$3/$4');
                    else if (v.length > 5)
                        v = v.replace(/(\d{2})(\d{3})(\d{0,3})/, '$1.$2.$3');
                    else if (v.length > 2)
                        v = v.replace(/(\d{2})(\d{0,3})/, '$1.$2');
                }
            }

            else if (tipo === 'telefone') {
                if (v.length > 11) v = v.slice(0, 11);
                v = v.length > 10
                    ? v.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3')
                    : v.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
            }

            else if (tipo === 'cep') {
                if (v.length > 8) v = v.slice(0, 8);
                if (v.length > 5) v = v.replace(/(\d{5})(\d{1,3})/, '$1-$2');
            }

            e.target.value = v;
        });
    });
});
