(function () {
    function updateDocumentationFields(container, documentType) {
        const answerGroup = container.querySelector('.documented-by-other-company-group');
        const companyGroup = container.querySelector('.documenting-company-group');
        const answer = container.querySelector('.documented-by-other-company');
        const company = container.querySelector('.documenting-company');
        const isUndocumented = documentType === 'Undocumented';

        answerGroup.style.display = isUndocumented ? '' : 'none';
        answer.required = isUndocumented;

        if (!isUndocumented) {
            answer.value = '';
            company.value = '';
        }

        const isDocumentedByOtherCompany = isUndocumented && answer.value.toLowerCase() === 'true';
        companyGroup.style.display = isDocumentedByOtherCompany ? '' : 'none';
        company.required = isDocumentedByOtherCompany;

        if (!isDocumentedByOtherCompany) {
            company.value = '';
        }

        if (window.jQuery) {
            window.jQuery(answer).trigger('change.select2');
            window.jQuery(company).trigger('change.select2');
        }
    }

    function initialize() {
        document.querySelectorAll('.check-voucher-documentation').forEach(function (container) {
            const typeSelector = document.querySelector('.check-voucher-document-type');
            const answer = container.querySelector('.documented-by-other-company');

            function update() {
                updateDocumentationFields(container, typeSelector ? typeSelector.value : container.dataset.documentType);
            }

            update();

            if (window.jQuery) {
                if (typeSelector) {
                    window.jQuery(typeSelector).on('change.checkVoucherDocumentation', update);
                }

                window.jQuery(answer).on('change.checkVoucherDocumentation', update);
                return;
            }

            if (typeSelector) {
                typeSelector.addEventListener('change', update);
            }

            answer.addEventListener('change', update);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialize);
    }
    else {
        initialize();
    }
})();
