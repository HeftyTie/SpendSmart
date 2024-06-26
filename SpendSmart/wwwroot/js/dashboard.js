var ctx = $("#budgetAllocationChart")[0].getContext('2d');
var chartDataJson = JSON.parse(document.getElementById("pieChartData").value);
var dataValues = Object.values(chartDataJson);

var budgetAllocationText = (chart) => {
    var dataSum = chart.data.datasets[0].data.reduce((sum, value, index) => {
        return !chart.getDatasetMeta(0).data[index].hidden ? sum + value : sum;
    }, 0);

    var ratioText;
    if (dataValues.every(val => val === 0)) {
        ratioText = 'No data to display';
    } else if (dataSum === 0) {
        ratioText = 'No data selected';
    } else {
        var percentageValues = chart.data.datasets[0].data.map(a => Math.round(a / dataSum * 100));
        var colors = ['#2ecc71', '#e74c3c', '#3498db'];
        var activeColors = colors.filter((color, index) => !chart.getDatasetMeta(0).data[index].hidden);

        var spans = [];
        for (var i = 0; i < percentageValues.length; i++) {
            var span = document.createElement('span');
            span.style.color = activeColors[i];
            span.innerHTML = percentageValues[i];
            spans.push(span);
        }

        spans.sort((a, b) => b.innerHTML - a.innerHTML);

        ratioText = "Current Ratio: ";
        for (var i = 0; i < spans.length; i++) {
            ratioText += spans[i].outerHTML;
            if (i < spans.length - 1) {
                ratioText += ' / ';
            }
        }

        ratioText += '<span> %</span>';
    }

    $("#ratioText").html(ratioText);
}

var budgetAllocationChart = new Chart(ctx, {
    type: 'pie',
    data: {

        labels: ['Credit', 'Debit', 'Savings'],
        datasets: [{
            data: dataValues,
            backgroundColor: [
                'rgba(46, 204, 113, 0.6)',
                'rgba(231, 76, 60, 0.6)',
                'rgba(52, 152, 219, 0.6)',
            ],
            borderColor: [
                'rgba(46, 204, 113, 1)',
                'rgba(231, 76, 60, 1)',
                'rgba(52, 152, 219, 1)',
            ],
            borderWidth: 1
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'none' 
            }
        },
    },
    plugins: [{
        afterUpdate: function (chart) {
            budgetAllocationText(chart); 
        }
    }]
});

function dataSort(a, b) {
    const percentageA = a.originalValue / a.maxValue * 100;
    const percentageB = b.originalValue / b.maxValue * 100;
    return percentageB - percentageA;
}

const colors = [
    'rgba(255, 99, 132, 1)',  // Red
    'rgba(255, 205, 86, 1)',  // Yellow
    'rgba(46, 204, 113, 1)',  // Green
    'rgba(54, 162, 235, 1)',  // Blue
    'rgba(54, 162, 235, 0.6)' // Light Blue
]

const createDataset = (label, originalValue, maxValue) => {
    let negativeRingColor = 'rgba(255, 255, 255, 0)';
    let ringColor;

    // Store original value to avoid modifying it
    var currentValue = originalValue;

    if (maxValue < 0) {
        // Handle negative maxValue cases
        if (currentValue / maxValue >= 0.8) {
            ringColor = colors[0];
        } else if (currentValue / maxValue >= 0.4) {
            ringColor = colors[1];
        } else {
            ringColor = colors[2];
        }
    } else {
        // Handle positive maxValue cases
        if (currentValue > maxValue) {
            currentValue = currentValue % maxValue;
            ringColor = colors[3];
            negativeRingColor = colors[4];
        } else if (currentValue / maxValue > 0.8) {
            ringColor = colors[2];
        } else if (currentValue / maxValue > 0.4) {
            ringColor = colors[1];
        } else {
            ringColor = colors[0];
        }
    }

    if (maxValue < 0) {
        var temp = ringColor;
        ringColor = negativeRingColor;
        negativeRingColor = temp;
    }

    if (currentValue > 0 && maxValue < 0) {
        ringColor = colors[2];
    }

    if (currentValue < 0 && maxValue > 0) {
        ringColor = colors[0];
        negativeRingColor = colors[0];
    }

    return {
        label: label,
        data: [currentValue, maxValue - currentValue],
        backgroundColor: [ringColor, negativeRingColor],
        borderWidth: 0,
        originalValue: originalValue,
        maxValue: maxValue,
        hoverOffset: 10
    };
}
// Get data from hidden fields
var savingsChartData = JSON.parse(document.getElementById("savingsChartData").value);
var budgetsChartData = JSON.parse(document.getElementById("budgetsChartData").value);

// Create datasets for Savings and Budgets charts
const savingsData = {
    datasets: savingsChartData.map(item => {
        return createDataset(item.Name, item.Balance, item.Goal);
    })
};

savingsData.datasets.sort(dataSort);

const budgetsData = {
    datasets: budgetsChartData.map(item => {
        return createDataset(item.Name, item.Balance, item.Goal);
    })
};

budgetsData.datasets.sort(dataSort);

// Base config for all doughnut charts
const config = {
    type: 'doughnut',
    options: {
        cutout: '30%',
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            tooltip: {
                enabled: false // Disable tooltip
            }
        },
        layout: {
            padding: {
                top: 10,
                bottom: 10,
                left: 10,
                right: 10
            }
        }
    }
};

// Clone config for Savings and Budgets charts with respective data
const savingsConfig = Object.assign({}, config, { data: savingsData });
const budgetsConfig = Object.assign({}, config, { data: budgetsData });

// Create Savings and Budgets charts
const savingsChart = new Chart($('#Savings-chart')[0], savingsConfig)
const budgetsChart = new Chart($('#Budgets-chart')[0], budgetsConfig)

// Update data text based on the selected elements
const updateDataText = (elements, data, textContainer) => {
    const dataText = document.getElementById(textContainer);
    dataText.innerHTML = '';

    if (data.datasets.length === 0) {
        dataText.innerHTML = 'No data for graph';
        return;
    }

    if (elements.length) {
        elements.forEach(element => {
            const datasetIndex = element.datasetIndex;
            const dataset = data.datasets[datasetIndex];
            const currentValue = dataset.originalValue;
            const maxValue = dataset.maxValue;
            const currentValueAsCurrency = currentValue.toLocaleString('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 })
            const maxValueAsCurrency = maxValue.toLocaleString('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 })
            const percentage = Math.round((currentValue / maxValue) * 100);
            const text = `<span>${dataset.label}: ${currentValueAsCurrency} / ${maxValueAsCurrency} (${percentage}%)</span><br>`;
            dataText.innerHTML += text;
        });
    } else {
        data.datasets.forEach(dataset => {
            const currentValue = dataset.originalValue;
            const maxValue = dataset.maxValue;
            const percentage = Math.round((currentValue / maxValue) * 100);
            const currentValueAsCurrency = currentValue.toLocaleString('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 })
            const maxValueAsCurrency = maxValue.toLocaleString('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 })
            const text = `<span>${dataset.label}: ${currentValueAsCurrency} / ${maxValueAsCurrency} (${percentage}%)</span><br>`;
            dataText.innerHTML += text;
        });
    }
};

// Update data on page load
updateDataText([], savingsData, "Savings-text");
updateDataText([], budgetsData, "Budgets-text");

var selectedMonth = $('#dateRangePicker')[0].value;
var monthDates;

var year = parseInt(selectedMonth.split('-')[0]);
var month = parseInt(selectedMonth.split('-')[1]);

// Get the number of days in the selected month
var daysInMonth = new Date(year, month, 0).getDate();
var daysArray = Array.from({ length: daysInMonth }, (v, k) => k + 1);

var transactions = JSON.parse(document.getElementById("transactionsData").value);

var amountSpent = new Array(daysInMonth).fill(0);
var amountGained = new Array(daysInMonth).fill(0);

transactions.forEach(transaction => {
    var date = new Date(transaction.Date);
    var day = date.getDate();
    var amount = parseFloat(transaction.Amount);

    if (amount > 0) {
        amountGained[day - 1] += amount;
    } else {
        amountSpent[day - 1] += amount; 
    }
});

var balance = [];
for (var i = 0; i < daysInMonth; i++) {
    if (i === 0) {
        balance.push(amountGained[i] + amountSpent[i]); 
    } else {
        balance.push(balance[i - 1] + amountGained[i] + amountSpent[i]);
    }
}

ctx = document.getElementById("transactionChart").getContext('2d');
var transactionData = {
    labels: daysArray,
    datasets: [
        {
            label: 'Spent',
            data: amountSpent,
            borderColor: '#e74c3c',
            backgroundColor: 'rgba(231, 76, 60, 0.2)',
            fill: false
        },
        {
            label: 'Gained',
            data: amountGained,
            borderColor: '#2ecc71',
            backgroundColor: 'rgba(46, 204, 113, 0.2)',
            fill: false
        },
        {
            label: 'Balance',
            data: balance,
            borderColor: '#3498db',
            backgroundColor: 'rgba(52, 152, 219, 0.2)',
            fill: false
        }
    ]
};

var transactionChart = new Chart(ctx, {
    type: 'line',
    data: transactionData,
    options: {
        responsive: true,
        scales: {
            x: {
                title: {
                    display: true,
                    text: 'Day of the Month'
                },
                grid: {
                    display: false
                }
            },
            y: {
                beginAtZero: true,
                title: {
                    display: true,
                    text: 'Amount'
                },
                grid: {
                    display: true
                },
                ticks: {
                    callback: function (value) {
                        return '$' + value.toFixed(2);
                    }
                }
            }
        }
    }
});