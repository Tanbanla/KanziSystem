window.cmMomentFormat = function(d){
  if(!d) return '';
  try{
    // Try parse ISO or /Date(x)/ from backend
    const dt = new Date(d);
    if(isNaN(dt.getTime())) return '';
    const pad = n => n.toString().padStart(2,'0');
    return `${dt.getFullYear()}-${pad(dt.getMonth()+1)}-${pad(dt.getDate())} ${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
  }catch{ return ''; }
};
