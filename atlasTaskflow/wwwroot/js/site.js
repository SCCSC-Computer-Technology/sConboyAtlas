// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("click", function (event) {
    // show or hide a password from the login and register forms
    const button = event.target.closest("[data-password-toggle]");

    if (!button) {
        return;
    }

    const input = document.getElementById(button.dataset.passwordToggle);

    if (!input) {
        return;
    }

    const isPassword = input.type === "password";
    input.type = isPassword ? "text" : "password";
    button.setAttribute("aria-label", isPassword ? "Hide password" : "Show password");
});

document.querySelectorAll("[data-sortable-table]").forEach(function (table) {
    // add sorting and calendar hover behavior to task tables
    const buttons = table.querySelectorAll("[data-sort]");
    const rows = table.querySelectorAll("tbody tr[data-date]");

    rows.forEach(function (row) {
        row.addEventListener("mouseenter", function () {
            const calendarDay = document.querySelector(`[data-calendar-date="${row.dataset.date}"]`);
            if (calendarDay) {
                calendarDay.classList.add("calendar-hover");
            }
        });

        row.addEventListener("mouseleave", function () {
            const calendarDay = document.querySelector(`[data-calendar-date="${row.dataset.date}"]`);
            if (calendarDay) {
                calendarDay.classList.remove("calendar-hover");
            }
        });
    });

    buttons.forEach(function (button) {
        button.addEventListener("click", function () {
            // sort the table rows using the selected data attribute
            const key = button.dataset.sort;
            const currentDirection = button.dataset.direction === "asc" ? "desc" : "asc";
            buttons.forEach(function (otherButton) {
                otherButton.removeAttribute("data-direction");
            });
            button.dataset.direction = currentDirection;

            const body = table.querySelector("tbody");
            const rows = Array.from(body.querySelectorAll("tr"));
            rows.sort(function (first, second) {
                const firstValue = first.dataset[key] || "";
                const secondValue = second.dataset[key] || "";
                return firstValue.localeCompare(secondValue, undefined, { numeric: true }) * (currentDirection === "asc" ? 1 : -1);
            });
            rows.forEach(function (row) {
                body.appendChild(row);
            });
        });
    });
});

document.querySelectorAll("[data-completion-toggle]").forEach(function (checkbox) {
    checkbox.addEventListener("change", function () {
        // update the row right away before submitting the form
        const row = checkbox.closest("tr");
        const name = row.querySelector(".focus-task-name");
        const status = row.querySelector("[data-task-status]");

        row.classList.toggle("focus-row-complete", checkbox.checked);
        name.classList.toggle("focus-task-name-complete", checkbox.checked);
        status.textContent = checkbox.checked ? "Complete" : "Open";
        checkbox.disabled = true;

        window.setTimeout(function () {
            checkbox.form.submit();
        }, 180);
    });
});
