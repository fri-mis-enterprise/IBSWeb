$(function () {
    $('form[data-cos-product-id]').each(function () {
        const form = this;
        let confirmed = false;
        let confirmationPending = false;

        $(form).on('submit', function (event) {
            if (confirmed || event.isDefaultPrevented()) {
                return;
            }

            const cosProductId = Number(form.dataset.cosProductId);
            const differentProducts = $(form).find('select[name="PurchaseOrderIds"] option:selected')
                .toArray()
                .filter(option => Number($(option).data('product-id')) !== cosProductId);

            if (differentProducts.length === 0) {
                return;
            }

            event.preventDefault();
            $('.loader-container').hide();
            $('body').removeClass('loading');
            sessionStorage.removeItem('isSubmitting');

            if (confirmationPending) {
                return;
            }

            const content = $('<div class="text-start"></div>');
            content.append($('<div class="bg-light border rounded p-3 mb-3"></div>')
                .append($('<div class="text-muted small mb-1"></div>').text('COS product'))
                .append($('<div class="fw-semibold text-break"></div>').text(form.dataset.cosProductName)));
            content.append($('<p class="mb-2"></p>').text('These selected POs have a different product:'));

            const purchaseOrders = $('<ul class="list-group mb-3"></ul>');
            differentProducts.forEach(option => {
                purchaseOrders.append($('<li class="list-group-item text-break"></li>').text(option.textContent));
            });
            content.append(purchaseOrders);
            content.append($('<p class="text-muted small mb-0"></p>')
                .text('Blended products may use different PO products. Please confirm these selections are intentional.'));

            const submitter = event.originalEvent?.submitter;
            confirmationPending = true;
            Swal.fire({
                title: 'Different products selected',
                icon: 'warning',
                html: content[0],
                width: '40rem',
                showCancelButton: true,
                confirmButtonText: 'Continue with these POs',
                cancelButtonText: 'Review selection',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d',
                reverseButtons: true,
                focusCancel: true
            }).then(result => {
                confirmationPending = false;
                if (!result.isConfirmed) {
                    return;
                }

                confirmed = true;
                if (submitter) {
                    form.requestSubmit(submitter);
                } else {
                    form.requestSubmit();
                }
                confirmed = false;
            });
        });
    });
});
