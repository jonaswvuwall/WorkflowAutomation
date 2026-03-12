import type { WorkflowAction } from '../types';

const ACTION_PARAMS: Record<string, { key: string; label: string; multiline?: boolean }[]> = {
  create_file:  [
    { key: 'path',    label: 'Pfad (z.B. C:/output/result.txt)' },
    { key: 'content', label: 'Inhalt', multiline: true },
  ],
  copy_file:    [
    { key: 'source',      label: 'Quelle' },
    { key: 'destination', label: 'Ziel' },
  ],
  move_file:    [
    { key: 'source',      label: 'Quelle' },
    { key: 'destination', label: 'Ziel' },
  ],
  delete_file:  [
    { key: 'path', label: 'Pfad' },
  ],
  log:          [
    { key: 'message', label: 'Nachricht' },
  ],
  send_webhook: [
    { key: 'url',  label: 'URL' },
    { key: 'body', label: 'Body (JSON)', multiline: true },
  ],
};

export function renderActionRow(container: HTMLElement, initial?: WorkflowAction): void {
  container.innerHTML = '';

  const row = document.createElement('div');
  row.className = 'action-row';

  const typeSel = document.createElement('select');
  typeSel.className = 'row-action-type';
  Object.keys(ACTION_PARAMS).forEach(t => {
    const opt = document.createElement('option');
    opt.value = t;
    opt.textContent = t;
    typeSel.appendChild(opt);
  });

  const paramsDiv = document.createElement('div');
  paramsDiv.className = 'action-params';

  const renderParams = (type: string, existing?: Record<string, string>) => {
    paramsDiv.innerHTML = '';
    (ACTION_PARAMS[type] ?? []).forEach(({ key, label, multiline }) => {
      const wrap = document.createElement('div');
      wrap.className = 'param-field';
      const lbl = document.createElement('label');
      lbl.textContent = label;
      const input = multiline
        ? Object.assign(document.createElement('textarea'), { rows: 3 })
        : document.createElement('input');
      input.dataset['paramKey'] = key;
      input.placeholder = key;
      if (existing?.[key]) input.value = existing[key];
      wrap.append(lbl, input);
      paramsDiv.appendChild(wrap);
    });
  };

  if (initial?.type) typeSel.value = initial.type;
  renderParams(typeSel.value, initial?.parameters);
  typeSel.addEventListener('change', () => renderParams(typeSel.value));

  row.append(typeSel, paramsDiv);
  container.appendChild(row);
}

export function getAction(container: HTMLElement): WorkflowAction {
  const row = container.querySelector<HTMLElement>('.action-row')!;
  const type = row.querySelector<HTMLSelectElement>('.row-action-type')?.value ?? '';
  const parameters: Record<string, string> = {};
  row.querySelectorAll<HTMLElement>('[data-param-key]').forEach(el => {
    parameters[el.dataset['paramKey']!] = (el as HTMLInputElement | HTMLTextAreaElement).value;
  });
  return { type, parameters };
}
