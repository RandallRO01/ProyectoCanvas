function validateForm() {
    const courseName = document.getElementById('course_name').value;
    const courseDescription = document.getElementById('course_description').value;
    const courseImage = document.getElementById('course_image').value;
    const courseYear = document.getElementById('course_year').value;
    const courseTerm = document.getElementById('course_term').value;
    let valid = true;

    if (courseName.lenght > 50 || !courseDescription || !courseImage || !courseYear || !courseTerm) {
        valid = false;
    }

    if (!valid) {
        document.getElementById('error-messsage').style.display = 'block';
        return false;
    }

    return true;

}
