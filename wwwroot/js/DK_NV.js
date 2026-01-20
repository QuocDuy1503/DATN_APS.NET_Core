(function () {
    const form = document.querySelector("form.form");
    const alertBox = document.getElementById("formAlert");
    const successBox = document.getElementById("successAlert");

    if (!form) return;

    function ensureErrorNode(field) {
        let err = field.querySelector(".error-text");
        if (!err) {
            err = document.createElement("div");
            err.className = "error-text";
            field.appendChild(err);
        }
        return err;
    }

    function isEmpty(el) {
        return !el.value || el.value.trim() === "";
    }

    function validateField(control) {
        const field = control.closest(".field");
        if (!field) return true;

        const required = control.hasAttribute("required");
        const type = (control.getAttribute("type") || "").toLowerCase();

        let ok = true;
        let msg = "";

        // Required
        if (required && isEmpty(control)) {
            ok = false;
            msg = "Trường này bắt buộc.";
        }

        // Email format
        if (ok && type === "email" && !isEmpty(control)) {
            ok = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(control.value.trim());
            if (!ok) msg = "Email không hợp lệ.";
        }

        // Phone format (basic)
        if (ok && type === "tel" && !isEmpty(control)) {
            ok = /^[0-9]{9,11}$/.test(control.value.trim());
            if (!ok) msg = "SĐT không hợp lệ (9–11 số).";
        }

        // GPA rule (0..4)
        if (ok && control.name === "gpa" && !isEmpty(control)) {
            const v = parseFloat(control.value);
            ok = !Number.isNaN(v) && v >= 0 && v <= 4;
            if (!ok) msg = "GPA phải từ 0.00 đến 4.00.";
        }

        const err = ensureErrorNode(field);
        if (!ok) {
            field.classList.add("has-error");
            err.textContent = msg;
        } else {
            field.classList.remove("has-error");
            err.textContent = "";
        }

        return ok;
    }

    const controls = form.querySelectorAll("input, select, textarea");

    // Realtime validate
    controls.forEach(el => {
        el.addEventListener("input", () => validateField(el));
        el.addEventListener("change", () => validateField(el));
        el.addEventListener("blur", () => validateField(el));
    });

    // Submit
    form.addEventListener("submit", function (e) {
        e.preventDefault(); // demo front-end (chưa có backend)

        let okAll = true;
        let firstInvalid = null;

        controls.forEach(el => {
            const ok = validateField(el);
            if (!ok && !firstInvalid) firstInvalid = el;
            okAll = okAll && ok;
        });

        if (!okAll) {
            alertBox.hidden = false;
            successBox.hidden = true;
            firstInvalid?.focus();
            firstInvalid?.scrollIntoView({ behavior: "smooth", block: "center" });
            return;
        }

        // SUCCESS
        alertBox.hidden = true;
        successBox.hidden = false;

        // reset + clear errors
        form.reset();
        document.querySelectorAll(".has-error").forEach(f => f.classList.remove("has-error"));
        document.querySelectorAll(".error-text").forEach(e => e.textContent = "");

        successBox.scrollIntoView({ behavior: "smooth", block: "center" });
    });
})();

