document.addEventListener('DOMContentLoaded', function () {
    const container = document.getElementById('enderecos-container');
    const btnAdd = document.getElementById('btn-adicionar-endereco');
    const templateHolder = document.getElementById('endereco-template');

    function reindex() {
        const wrappers = container.querySelectorAll('.endereco-wrapper');
        wrappers.forEach((wrap, idx) => {
            const item = wrap.querySelector('.endereco-item');
            if (!item) return;

            // inputs, selects, textareas
            const fields = item.querySelectorAll('input, select, textarea');
            fields.forEach(field => {
                // original property name (when template is used it will be like 'Logradouro')
                let prop = field.name || field.getAttribute('name') || field.id || '';
                // if it already contains Enderecos[ then try to extract final property
                if (prop.includes('].')) {
                    prop = prop.split('].').pop();
                }
                // if contains dot
                if (prop.includes('.')) {
                    prop = prop.split('.').pop();
                }
                // build new name and id
                const newName = `Enderecos[${idx}].${prop}`;
                const newId = `Enderecos_${idx}__${prop}`;
                field.name = newName;
                field.id = newId;
            });

            // update labels
            const labels = item.querySelectorAll('label');
            labels.forEach(label => {
                let prop = label.getAttribute('for') || '';
                if (prop.includes('].')) prop = prop.split('].').pop();
                if (prop.includes('.')) prop = prop.split('.').pop();
                const newFor = `Enderecos_${idx}__${prop}`;
                label.setAttribute('for', newFor);
            });

            // update validation spans (data-valmsg-for)
            const valSpans = item.querySelectorAll('[data-valmsg-for]');
            valSpans.forEach(s => {
                let field = s.getAttribute('data-valmsg-for') || '';
                if (field.includes('].')) field = field.split('].').pop();
                if (field.includes('.')) field = field.split('.').pop();
                s.setAttribute('data-valmsg-for', `Enderecos[${idx}].${field}`);
            });
        });
    }

    function wireRemoveButtons(context) {
        const removes = context.querySelectorAll('.btn-remover-endereco');
        removes.forEach(btn => {
            btn.removeEventListener('click', removeHandler);
            btn.addEventListener('click', removeHandler);
        });
    }

    function removeHandler(e) {
        const wrapper = e.target.closest('.endereco-wrapper');
        if (wrapper) {
            wrapper.remove();
            reindex();
        }
    }

    if (btnAdd) {
        btnAdd.addEventListener('click', function () {
            if (!templateHolder) return;
            const html = templateHolder.innerHTML.trim();
            if (!html) return;
            const temp = document.createElement('div');
            temp.innerHTML = html;
            // wrap in endereco-wrapper
            const wrapper = document.createElement('div');
            wrapper.className = 'endereco-wrapper';
            // If the template contains top-level endereco-item, append that element
            // otherwise append all children
            const item = temp.querySelector('.endereco-item');
            if (item) wrapper.appendChild(item.cloneNode(true)); else wrapper.innerHTML = html;

            container.appendChild(wrapper);
            reindex();
            wireRemoveButtons(wrapper);
        });
    }

    // Wire existing remove buttons
    wireRemoveButtons(container);
    // Reindex on load so existing EditorFor items are consistent
    reindex();
});
