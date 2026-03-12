import type { Condition } from '../types';

const FIELDS_BY_TRIGGER: Record<string, string[]> = {
  file_created:  ['filename', 'extension', 'filesize', 'filepath'],
  file_modified: ['filename', 'extension', 'filesize', 'filepath'],
  manual:        [],
};

const OPERATORS = [
  'equals', 'not_equals',
  'contains', 'not_contains',
  'starts_with', 'ends_with',
  'gt', 'lt', 'gte', 'lte',
  'regex',
];

export function renderConditionRow(
  container: HTMLElement,
  triggerType: string,
  initial?: Condition,
): void {
  const fields = FIELDS_BY_TRIGGER[triggerType] ?? [];

  const row = document.createElement('div');
  row.className = 'editor-row';

  const fieldSel = document.createElement('select');
  fieldSel.className = 'row-field';
  if (fields.length === 0) {
    const opt = document.createElement('option');
    opt.value = '';
    opt.textContent = '(kein Feld verfügbar)';
    fieldSel.appendChild(opt);
  } else {
    fields.forEach(f => {
      const opt = document.createElement('option');
      opt.value = f;
      opt.textContent = f;
      fieldSel.appendChild(opt);
    });
  }
  if (initial?.field) fieldSel.value = initial.field;

  const opSel = document.createElement('select');
  opSel.className = 'row-operator';
  OPERATORS.forEach(op => {
    const opt = document.createElement('option');
    opt.value = op;
    opt.textContent = op;
    opSel.appendChild(opt);
  });
  if (initial?.operator) opSel.value = initial.operator;

  const valueInput = document.createElement('input');
  valueInput.className = 'row-value';
  valueInput.placeholder = 'Wert';
  if (initial?.value) valueInput.value = initial.value;

  const removeBtn = document.createElement('button');
  removeBtn.type = 'button';
  removeBtn.className = 'remove-row-btn';
  removeBtn.textContent = '×';
  removeBtn.addEventListener('click', () => row.remove());

  row.append(fieldSel, opSel, valueInput, removeBtn);
  container.appendChild(row);
}

export function getConditions(container: HTMLElement): Condition[] {
  return Array.from(container.querySelectorAll<HTMLElement>('.editor-row')).map(row => ({
    field:    (row.querySelector<HTMLSelectElement>('.row-field')?.value ?? ''),
    operator: (row.querySelector<HTMLSelectElement>('.row-operator')?.value ?? ''),
    value:    (row.querySelector<HTMLInputElement>('.row-value')?.value ?? ''),
  }));
}

export function clearConditions(container: HTMLElement): void {
  container.innerHTML = '';
}
