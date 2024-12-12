let currentDate = new Date();
let calendarData = {};

// Render calendar for the current month
function renderCalendar() {
    const month = currentDate.getMonth();
    const year = currentDate.getFullYear();
    const firstDay = new Date(year, month, 1).getDay();
    const totalDays = new Date(year, month + 1, 0).getDate();

    // Update the month header
    document.getElementById("current-month").innerText = `${getMonthName(month)} ${year}`;

    // Create the days grid
    let calendarDays = '';
    for (let i = 0; i < firstDay; i++) {
        calendarDays += '<div class="empty-day"></div>'; // Empty cells for alignment
    }
    for (let i = 1; i <= totalDays; i++) {
        const value = calendarData[i] || ''; // Get the value for the day
        let className = '';
        if (value < 0) {
            className = 'expense'; // Color for expenses
        } else if (value > 0) {
            className = 'income'; // Color for income
        }

        calendarDays += `<div class="calendar-day ${className}" onclick="showDayDetails(${i})">
            ${i} <br> ${value}
        </div>`;
    }

    document.getElementById("calendar-days").innerHTML = calendarDays;
}

// Add new data to a day
function addData(day, value) {
    calendarData[day] = value;
    renderCalendar();
}

// Show a prompt to add data for a specific day
function showDayDetails(day) {
    const value = prompt(`Enter value for day ${day}:`);
    if (value) {
        addData(day, parseFloat(value));
    }
}

// Helper function to get month name
function getMonthName(month) {
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    return monthNames[month];
}

// Button to navigate to previous month
document.getElementById("prev-month").addEventListener("click", function () {
    currentDate.setMonth(currentDate.getMonth() - 1);
    renderCalendar();
});

// Button to navigate to next month
document.getElementById("next-month").addEventListener("click", function () {
    currentDate.setMonth(currentDate.getMonth() + 1);
    renderCalendar();
});

// Initial render
renderCalendar();
