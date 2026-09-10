export function aplicarTema(esOscuro: boolean) {
  document.documentElement.classList.toggle('dark', esOscuro);
  document.documentElement.classList.toggle('ion-palette-dark', esOscuro);
  localStorage.setItem('tema_oscuro', esOscuro ? 'true' : 'false');
}

export function cargarTema(): boolean {
  const guardado = localStorage.getItem('tema_oscuro') === 'true';
  aplicarTema(guardado);
  return guardado;
}