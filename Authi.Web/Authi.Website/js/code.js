document.addEventListener('DOMContentLoaded', async () => {
    await document.fonts.ready;

    document.body.style.visibility = "visible";

    const main = document.getElementById('main');
    if (main) {
        main.addEventListener('wheel', e => {
            if (getComputedStyle(main)['overflowX'] == 'hidden') {
                return;
            }
            main.scrollLeft += e.deltaY;
        });
    }
});
