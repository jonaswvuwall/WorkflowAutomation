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

const ACTION_TYPES = Object.keys(ACTION_PARAMS);

export function renderActionRow(container: HTMLElement, initial?: WorkflowAction): void {
  const row = document.createElement('div');
  row.className = 'action-row';

  const header = document.createElement('div');
  header.className = 'action-row-header';

  const typeSel = document.createElement('select');
  typeSel.className = 'row-action-type';
  ACTION_TYPES.forEach(t => {
    const opt = document.createElement('option');
    opt.value = t;
    opt.textContent = t;
    typeSel.appendChild(opt);
  });

  const removeBtn = document.createElement('button');
  removeBtn.type = 'button';
  removeBtn.className = 'remove-row-btn';
  removeBtn.textContent = '×';
  removeBtn.addEventListener('click', () => row.remove());

  header.append(typeSel, removeBtn);

  const paramsDiv = document.createElement('div');
  paramsDiv.className = 'action-params';

  const renderParams = (type: string, existing?: Record<string, string>) => {
    paramsDiv.innerHTML = '';
    const defs = ACTION_PARAMS[type] ?? [];
    defs.forEach(({ key, label, multiline }) => {
      const wrap = document.createElement('div');
      wrap.className = 'param-field';

      const lbl = document.createElement('label');
      lbl.textContent = label;

      let input: HTMLInputElement | HTMLTextAreaElement;
      if (multiline) {
        input = document.createElement('textarea');
        input.rows = 3;
      } else {
        input = document.createElement('input');
      }
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

  row.append(header, paramsDiv);
  container.appendChild(row);
}

export function getActions(container: HTMLElement): WorkflowAction[] {
  return Array.from(container.querySelectorAll<HTMLElement>('.action-row')).map(row => {
    const type = row.querySelector<HTMLSelectElement>('.row-action-type')?.value ?? '';
    const parameters: Record<string, string> = {};
    row.querySelectorAll<HTMLElement>('[data-param-key]').forEach(el => {
      const key = el.dataset['paramKey']!;
      parameters[key] = (el as HTMLInputElement | HTMLTextAreaElement).value;
    });
    return { type, parameters };
  });
}

export function clearActions(container: HTMLElement): void {
  container.innerHTML = '';
}
