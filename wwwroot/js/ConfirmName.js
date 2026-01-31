(function(){
  const root = document.getElementById('confirm-name');
  if(!root) return;
  const role = root.getAttribute('data-role') || 'UserPUR';

  const els = {
    tenHang: document.getElementById('tenHang'),
    soDon: document.getElementById('soDon'),
    trangThai: document.getElementById('trangThai'),
    btnSearch: document.getElementById('btnSearch'),
    tbody: document.getElementById('confirmTableBody'),
    resultCount: document.getElementById('resultCount'),
    prev: document.getElementById('prevPage'),
    next: document.getElementById('nextPage'),
    pageInfo: document.getElementById('pageInfo')
  };

  let state = { pageIndex: 1, pageSize: 20, total: 0 };

  function statusBadge(s){
    switch((s||'').toLowerCase()){
      case 'confirmed': return '<span class="status-badge status-confirmed">Đã xác nhậnn</span>';
      case 'confirming': return '<span class="status-badge status-confirming">Đang xác nhận</span>';
      case 'rejected': return '<span class="status-badge status-rejected">Từ chối</span>';
      default: return '<span class="status-badge status-draft">Mới</span>';
    }
  }

  function canEditTenHQ(){ return role === 'UserShip' || role === 'UserPUR'; }
  function canEditMaNB(){ return role === 'UserAcc' || role === 'UserPUR'; }
  function canApprove(){ return role === 'UserPUR'; }

  function renderRows(data){
    if(!data || data.length===0){
      els.tbody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">Không có dữ liệu</td></tr>';
      return;
    }
    els.tbody.innerHTML = data.map((r,i)=>{
      const idx = (state.pageIndex-1)*state.pageSize + i + 1;
      const tenHQ = canEditTenHQ() ? `<input class="form-control form-control-sm js-tenhq" data-id="${r.id}" value="${r.tenHaiQuan||''}" />` : `<div class="cell-sm">${r.tenHaiQuan||''}</div>`;
      const maNB = canEditMaNB() ? `<input class="form-control form-control-sm js-manb" data-id="${r.id}" value="${r.maHangNoiBo||''}" />` : `<div>${r.maHangNoiBo||''}</div>`;
      const actions = [
        canApprove() ? `<button class="btn btn-sm btn-success js-approve" data-id="${r.id}">Đồng ý</button>` : '',
        canApprove() ? `<button class="btn btn-sm btn-outline-danger js-reject" data-id="${r.id}">Từ chối</button>` : ''
      ].filter(Boolean).join(' ');
      const handler = [r.userShip && `Ship: ${r.userShip} (${formatDate(r.dtmUserShip)})`, r.userAcc && `Acc: ${r.userAcc} (${formatDate(r.dtmUserAcc)})`, r.userPur && `PUR: ${r.userPur} (${formatDate(r.dtmUserPur)})`].filter(Boolean).join('<br/>');
      return `<tr>
        <td class="text-center">${idx}</td>
        <td>${r.soDon||''}</td>
        <td>${tenHQ}</td>
        <td>${maNB}</td>
        <td class="text-center">${statusBadge(r.trangThai)}</td>
        <td>${formatDate(r.createDate)}</td>
        <td>${handler||''}</td>
        <td><div class="small text-muted">${r.note||''}</div><div class="text-danger small">${r.lyDo||''}</div></td>
        <td class="text-center">${actions}</td>
      </tr>`;
    }).join('');

    // attach events
    els.tbody.querySelectorAll('.js-tenhq').forEach(el=>{
      el.addEventListener('change', ()=> saveInline(parseInt(el.getAttribute('data-id')), { tenHaiQuan: el.value }));
    });
    els.tbody.querySelectorAll('.js-manb').forEach(el=>{
      el.addEventListener('change', ()=> saveInline(parseInt(el.getAttribute('data-id')), { maHangNoiBo: el.value }));
    });
    els.tbody.querySelectorAll('.js-approve').forEach(btn=> btn.addEventListener('click', ()=> approve(parseInt(btn.getAttribute('data-id')))));
    els.tbody.querySelectorAll('.js-reject').forEach(btn=> btn.addEventListener('click', ()=> reject(parseInt(btn.getAttribute('data-id')))));
  }

  function formatDate(d){
    if(window.cmMomentFormat){ return window.cmMomentFormat(d); }
    if(!d) return '';
    const dt = new Date(d);
    if(isNaN(dt.getTime())) return '';
    const pad = n => n.toString().padStart(2,'0');
    return `${dt.getFullYear()}-${pad(dt.getMonth()+1)}-${pad(dt.getDate())} ${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
  }

  async function search(){
    const body = {
      tenHang: els.tenHang.value.trim(),
      soDon: els.soDon.value.trim(),
      trangThai: els.trangThai.value,
      pageIndex: state.pageIndex,
      pageSize: state.pageSize
    };
    const res = await fetch('/Material/SearchConfirmName', {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body)
    });
    if(!res.ok){ console.error('Search failed'); return; }
    const data = await res.json();
    state.total = data.total || 0;
    state.pageIndex = data.pageIndex || 1;
    state.pageSize = data.pageSize || 20;
    els.resultCount.textContent = `Tổng: ${state.total}`;
    const totalPages = Math.max(1, Math.ceil(state.total/state.pageSize));
    els.pageInfo.textContent = `${state.pageIndex}/${totalPages}`;
    renderRows(data.data||[]);
  }

  async function saveInline(id, payload){
    const body = Object.assign({ id, role }, payload);
    const res = await fetch('/Material/SaveConfirmName', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(body)});
    if(!res.ok){ alert('Lưu thất bại'); }
  }

  async function approve(id){
    if(!confirm('Xác nhận đồng ý?')) return;
    const res = await fetch('/Material/ApproveConfirmName', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ id })});
    if(res.ok){ search(); } else { alert('Thao tác thất bại'); }
  }

  async function reject(id){
    const lyDo = prompt('Nh?p lý do t? ch?i:');
    if(lyDo === null) return;
    const res = await fetch('/Material/RejectConfirmName', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ id, lyDo })});
    if(res.ok){ search(); } else { alert('Thao tác thất bại'); }
  }

  els.btnSearch.addEventListener('click', ()=>{ state.pageIndex = 1; search(); });
  els.prev.addEventListener('click', ()=>{ if(state.pageIndex>1){ state.pageIndex--; search(); }});
  els.next.addEventListener('click', ()=>{ const totalPages = Math.max(1, Math.ceil(state.total/state.pageSize)); if(state.pageIndex<totalPages){ state.pageIndex++; search(); }});

  // initial search
  search();
})();
