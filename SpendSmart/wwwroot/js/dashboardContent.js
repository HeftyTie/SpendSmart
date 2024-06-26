$(function () {
    // Function to toggle goal form based on account type selection
    function toggleGoalForm() {
        $('.account-type').each(function () {
            var selectedType = $(this).val();
            var $goalForm = $(this).closest('.form-group').next('.goal-form'); 
            var $typeNameForm = $goalForm.closest('.form-group').next('.type-name-form');

            if (selectedType === 'Savings' || selectedType === 'Budget') {
                $goalForm.removeClass('d-none');
                $typeNameForm.removeClass('d-none');
            } else {
                $goalForm.addClass('d-none');
                $typeNameForm.addClass('d-none');
            }
        });
    }

    // Call toggle function on page load
    toggleGoalForm();

    // Call toggle function on account type change using event delegation
    $(document).on('change', '.account-type', function () {
        toggleGoalForm();
    });
});

$('#editModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var itemId = button.data('item-id');
    var modal = $(this);

    modal.modal('hide');
    $.ajax({
        url: '?handler=Update&id=' + itemId,
        type: 'GET',
        dataType: 'html',
        success: function (response) {
            modal.find('#formBody').html(response);
// Call toggle function after loading the edit form
        }
    });
});

$('#editModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var itemId = button.data('item-id');
    var modal = $(this);

    modal.modal('hide');
    $.ajax({
        url: '?handler=Update&id=' + itemId,
        type: 'GET',
        dataType: 'html',
        success: function (response) {
            modal.find('#formBody').html(response);
            function toggleGoalForm() {
                $('#editAccount').each(function () {
                    var selectedType = $(this).val();
                    var $goalForm = $('#editGoal');
                    var $typeNameForm = $('#editTypeName');

                    if (selectedType === 'Savings' || selectedType === 'Budget') {
                        $goalForm.removeClass('d-none');
                        $typeNameForm.removeClass('d-none');
                        console.log("remove");

                    } else {
                        $goalForm.addClass('d-none');
                        $typeNameForm.addClass('d-none');
                        console.log("add");
                    }
                });
            }

            // Call toggle function on page load
            toggleGoalForm();

            // Call toggle function on account type change using event delegation
            $(document).on('change', '#editAccount', function () {
                toggleGoalForm();
            }); 
        }
    });
});

$('#editModal').on('hidden.bs.modal', function () {
    $(this).find('#formBody').empty();
});

$(function () {
    const $deleteModal = $('#deleteModal');
    $deleteModal.on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var itemId = button.data('item-id');
        var $input = $deleteModal.find('#deleteItemId');
        $input.val(itemId);
    });
});

const $tableBody = $('.table-body');
const $searchBar = $('#searchInput');

$(function () {
    const originalOrder = $tableBody.find('tr').clone();

    $(document).on('click', '.btn-table', function () {
        const $btn = $(this);
        const $icon = $btn.find('i');

        if ($searchBar.val().length > 0) {
            $searchBar.val('');
            $tableBody.find('tr').show();
        }

        // Reset all icons to 'bi-arrow-down-up'
        $('.btn-table i').not($icon).removeClass('bi-arrow-up bi-arrow-down').addClass('bi-arrow-down-up');

        let sortOrder = 'asc';
        // Toggle sort order and update icon
        if ($icon.hasClass('bi-arrow-down-up')) {
            $icon.removeClass('bi-arrow-down-up').addClass('bi-arrow-up');
            sortOrder = 'asc';
        } else if ($icon.hasClass('bi-arrow-up')) {
            $icon.removeClass('bi-arrow-up').addClass('bi-arrow-down');
            sortOrder = 'desc';
        } else {
            $icon.removeClass('bi-arrow-down').addClass('bi-arrow-down-up');
            sortOrder = 'original';
        }

        let $sortedRows;
        const columnIndex = $btn.closest('th').index();
        const $rows = $tableBody.find('tr');

        switch (sortOrder) {
            case 'asc':
            case 'desc':
                $sortedRows = $rows.clone().sort((a, b) => {
                    let aValue = parseCellValue($(a).find('td').eq(columnIndex).text().trim());
                    let bValue = parseCellValue($(b).find('td').eq(columnIndex).text().trim());
                    return compareValues(aValue, bValue, sortOrder);
                });
                break;

            default:
                $sortedRows = originalOrder.clone();
                break;
        }

        // Replace the table body with the sorted rows
        $tableBody.empty().append($sortedRows);
    });
});

function parseCellValue(value) {
    // Check if the value is a date
    if (/\d{4}-\d{2}-\d{2}/.test(value)) {
        return new Date(value);
    }

    // Remove any dollar signs, parentheses, and commas
    let numericValue = value.replace(/[\$,]/g, '');
    if (numericValue.startsWith('(') && numericValue.endsWith(')')) {
        numericValue = '-' + numericValue.slice(1, -1);
    }
    numericValue = parseFloat(numericValue);
    return isNaN(numericValue) ? value.toLowerCase() : numericValue;
}


function compareValues(a, b, sortOrder) {
    // Check if the values are dates
    if (a instanceof Date && b instanceof Date) {
        return sortOrder === 'asc' ? a.getTime() - b.getTime() : b.getTime() - a.getTime();
    } else if (typeof a === 'number' && typeof b === 'number') {
        return sortOrder === 'asc' ? a - b : b - a;
    } else {
        if (a < b) {
            return sortOrder === 'asc' ? -1 : 1;
        }
        if (a > b) {
            return sortOrder === 'asc' ? 1 : -1;
        }
        return 0;
    }
}
$(function () {
    $searchBar.on('input', function () {
        const $rows = $tableBody.find('tr');
        const searchText = $(this).val().trim().toLowerCase();

        $rows.each(function () {
            const $row = $(this);
            const rowText = $row.text().toLowerCase();
            const wordsArray = rowText.split(/\s+/).filter(word => word.trim() !== '');

            let showRow = false;
            wordsArray.forEach(function (word) {
                if (rowText.includes(searchText)) {
                    showRow = true;
                }
            });

            $row.toggle(showRow);
        });
    });
});