// Dialog de confirmação reutilizável para ações destrutivas
export function ConfirmDialog({ mensagem, onConfirmar, onCancelar }) {
  return (
    <div className="overlay">
      <div className="dialog">
        <p>{mensagem}</p>
        <div className="dialog-acoes">
          <button className="btn-secundario" onClick={onCancelar}>
            Cancelar
          </button>
          <button className="btn-perigo" onClick={onConfirmar}>
            Confirmar
          </button>
        </div>
      </div>
    </div>
  );
}
