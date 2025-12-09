document.addEventListener('input', function (e) {
    const target = e.target;
    if (!(target instanceof HTMLElement)) return;

    // Only handle inputs and textareas
    if (!(target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement)) return;

    const tipo = target.getAttribute('data-maiuscula');
    if (!tipo) return;

    const original = target.value || '';
    let novo = original;

    if (tipo === 'tudo') {
        novo = original.toUpperCase();
    } else if (tipo === 'titulo') {
        novo = toTitleCase(original);
    } else {
        return;
    }

    if (novo !== original) {
        // try to preserve caret/selection as best as possible
        const start = target.selectionStart;
        const end = target.selectionEnd;
        const prevLen = original.length;

        target.value = novo;

        // compute new selection roughly at same logical position
        if (start !== null && end !== null) {
            const diff = novo.length - prevLen;
            try {
                const newStart = Math.max(0, start + diff);
                const newEnd = Math.max(0, end + diff);
                target.setSelectionRange(newStart, newEnd);
            } catch (err) {
                // ignore if setting selection fails
            }
        }
    }
});

function toTitleCase(str) {
    if (!str) return '';

    // small words that should remain lowercase unless first word
    const smallWords = new Set(['de','da','do','dos','das','e','em','no','na','nos','nas','a','o','as','os','por','para','com','sem']);

    // Split preserving whitespace
    const tokens = str.split(/(\s+)/);
    let wordIndex = 0; // counts word tokens (non-whitespace)

    for (let i = 0; i < tokens.length; i++) {
        const token = tokens[i];
        if (/^\s+$/.test(token)) continue; // whitespace

        // Handle hyphenated words (e.g., "são-paulo")
        const parts = token.split(/(-)/); // keep hyphens in array
        for (let p = 0; p < parts.length; p++) {
            const part = parts[p];
            if (part === '-') continue;
            // If part is punctuation or empty, skip
            if (!/[\p{L}\p{N}]/u.test(part)) continue;

            const lower = part.toLowerCase();
            let transformed;

            if (wordIndex > 0 && smallWords.has(lower)) {
                transformed = lower; // keep small words lowercase (unless first word)
            } else {
                // Capitalize first letter, keep rest lowercase
                // But handle cases like "o'neill" or names with apostrophes
                transformed = lower.replace(/(^|[^\p{L}\p{N}])([\p{L}])/u, function(m, pre, ch) {
                    return (pre || '') + ch.toUpperCase();
                });
            }

            parts[p] = transformed;
            wordIndex++;
        }

        tokens[i] = parts.join('');
    }

    return tokens.join('');
}
