// c = convention
// d = deduction
// o = observation
// r = reveal
// s = strategy

let deductionTrackers = [];

function makeSolutionPage(pageId, hpsml, triggers)
{
	hpsml = hpsml
		.replace(/«([^»]*?)»/g, (_, m) => `<span class='w'>${m}</span>`)
		.replace(/‹([^›]*?)›/g, (_, m) => `<span class='h'>${m}</span>`);

	let idAlloc = 1;
	let alloc;
	let revealed = [];
	let revealing = false;
	let solutionDiv = document.createElement('div');
	document.getElementById('main').appendChild(solutionDiv);

	function addAlloc(id, fnc)
	{
		if (!(id in alloc))
			alloc[id] = [];
		alloc[id].push(fnc);
	}

	function makeSolutionPageImpl(hpsml, eof)
	{
		let html = [], fncs = [], m;
		let tokens = [
			/\[(?<sblock>-?)(?<type>[cdors])(?<sid>\w*)(?:\s|(?=\]))/,
			/\]/,
			/\{(?<cblock>-?)(?<cid>\w*)(?:\/(?<crid>\w+))?(?:\s|(?=\}))/,
			/\}/,
			/▲(?<spid>\w+)/,
			/▼/,
			/$/
		];

		while (m = new RegExp(`^(?<pre>.*?)(?<c>${tokens.map(s => s.source).join('|')})`, 's').exec(hpsml))
		{
			html.push(m.groups.pre);
			hpsml = hpsml.substr(m[0].length);

			let nid;
			switch (m.groups.c[0])
			{
				case ']':
				case '}':
				case '▼':
				case undefined:
					if (eof !== m.groups.c[0])
						console.error(`Unexpected ${m.groups.c[0] || 'EOF'} (expected ${eof || 'EOF'})`);
					return { html: html.join(''), fncs: function() { fncs.forEach(f => { f(); }); }, rest: hpsml };

				case '[':
					nid = idAlloc++;
					let sid = m.groups.sid;
					html.push(`<button type='button' class='reveal ${m.groups.type}' id='b-${nid}'></button>`);
					let sTag = m.groups.sblock ? 'div' : 'span';
					html.push(`<${sTag} id='s-${nid}'></${sTag}>`);
					let sResult = makeSolutionPageImpl(hpsml, ']');
					fncs.push(function()
					{
						document.getElementById(`b-${nid}`).onclick = function()
						{
							document.getElementById(`s-${nid}`).innerHTML = sResult.html;
							sResult.fncs();
							if (sid && sid in alloc)
								for (let fnc of alloc[sid])
									fnc();
							document.getElementById(`b-${nid}`).remove();
							Array.from(document.getElementsByClassName(`r-${sid}`)).forEach(re =>
							{
								re.classList.add(`re-${sid}`);
								re.classList.add(`re`);
								if (`rem_${sid}` in re.dataset)
									addAlloc(re.dataset[`rem_${sid}`], function()
									{
										re.classList.remove(`re-${sid}`);
										if (Array.from(re.classList).every(cls => !cls.startsWith('re-')))
											re.classList.remove('re');
									});
							});
							let sort = Array.from(document.getElementsByClassName(`sort-${sid}`));
							let sortInf = sort.map(elem => [elem, elem.parentNode, elem.nextSibling]);
							if (sort.length > 0)
							{
								while (true)
								{
									let ix = sortInf.findIndex(ar => sortInf.some(ar2 => ar2[2] !== null && ar2[0] === ar[2]));
									if (ix === -1)
										break;
									sortInf[ix][2] = sortInf[sortInf.findIndex(ar2 => ar2[2] !== null && ar2[0] === sortInf[ix][2])][2];
								}
								sort.sort((a, b) => (a.dataset[`sort_${sid}`] | 0) - (b.dataset[`sort_${sid}`] | 0));
								for (let i = 0; i < sort.length; i++)
									sort[i].parentNode.removeChild(sort[i]);
								for (let i = 0; i < sort.length; i++)
									sortInf[i][1].insertBefore(sort[i], sortInf[i][2]);
							}
							if (!revealing)
							{
								revealed.push(nid);
								localStorage.setItem(`sol-${pageId}`, JSON.stringify(revealed));
							}
							if (triggers && sid in triggers)
								triggers[sid]();
							return false;
						};
					});
					hpsml = sResult.rest;
					break;

				case '{':
					nid = idAlloc++;
					let cTag = m.groups.cblock ? 'div' : 'span';
					html.push(`<${cTag} id='c-${nid}'></${cTag}>`);
					let cResult = makeSolutionPageImpl(hpsml, '}');
					let crid = m.groups.crid;

					// This function MUST NOT capture m
					function fillIn()
					{
						document.getElementById(`c-${nid}`).innerHTML = cResult.html;
						cResult.fncs();
						if (crid)
							addAlloc(crid, function() { document.getElementById(`c-${nid}`).remove(); });
					}

					if (m.groups.cid)
						addAlloc(m.groups.cid, fillIn);
					else
						fncs.push(fillIn);
					hpsml = cResult.rest;
					break;

				case '▲':
					let tabs = [], spid = m.groups.spid, m2, ls, lsBtn;
					let spResult = makeSolutionPageImpl(hpsml, '▼');
					let lsid = `sol-${pageId}-${spid}`;
					while (m2 = /^\s*•(?<tabid>\w+) (?<title>[^\r\n]+)[\r\n](?<html>[^•]*)/s.exec(spResult.html))
					{
						tabs.push(m2.groups);
						spResult.html = spResult.html.substr(m2[0].length);
					}

					html.push(`
						<div class='subpuzzles' id='sp-${spid}'>
							<div class='menu'>${tabs.map(tab => `<button id='sp-${spid}-t-${tab.tabid}'>${tab.title}</button>`).join('')}</div>
							${tabs.map(tab => `<div class='content' id='sp-${spid}-c-${tab.tabid}'>${tab.html}</div>`).join('')}
						</div>`);

					// This function MUST NOT capture m
					fncs.push(function()
					{
						spResult.fncs();
						for (let tab of tabs)
							document.getElementById(`sp-${spid}-t-${tab.tabid}`).onclick = function()
							{
								let wasSelected = document.querySelector(`#sp-${spid}-t-${tab.tabid}.selected`);
								Array.from(document.querySelectorAll(`#sp-${spid} > .menu > button`)).forEach(btn => { btn.classList.remove('selected'); });
								Array.from(document.querySelectorAll(`#sp-${spid} > .content`)).forEach(cnt => { cnt.classList.remove('selected'); });
								if (!wasSelected)
								{
									document.getElementById(`sp-${spid}-t-${tab.tabid}`).classList.add('selected');
									document.getElementById(`sp-${spid}-c-${tab.tabid}`).classList.add('selected');
									if (triggers && triggers[tab.tabid])
										triggers[tab.tabid]();
									if (triggers && triggers[spid])
										triggers[spid](tab.tabid);
									if (localStorage)
										localStorage.setItem(lsid, tab.tabid);
								}
								else
								{
									if (triggers && triggers[`${tab.tabid}-off`])
										triggers[`${tab.tabid}-off`]();
									if (triggers && triggers[spid])
										triggers[spid](null);
									if (localStorage)
										localStorage.removeItem(lsid);
								}
							};
						if (localStorage && (ls = localStorage.getItem(lsid)) && (lsBtn = document.getElementById(`sp-${spid}-t-${ls}`)))
							lsBtn.onclick();
					});
					hpsml = spResult.rest;
					break;
			}
		}
	}

	function initialSetup()
	{
		alloc = {};

		idAlloc = 1;
		let result = makeSolutionPageImpl(hpsml);
		if (result.rest.length > 0)
			console.error('unmatched brackets in HPSML — please check');

		solutionDiv.innerHTML = `
			<div id='sol-controls'>
				<button id='sol-reset' accesskey='r'>◀◀</button>
				<button id='sol-back' accesskey='w'>◀</button>
				<button id='sol-forward' accesskey='.'>▶</button>
				<button id='sol-expand' accesskey='a'>▶▶</button>
			</div>
			${result.html}`;
		result.fncs();

		document.getElementById('sol-reset').onclick = function()
		{
			for (let tracker of deductionTrackers)
				localStorage.removeItem(tracker);
			localStorage.removeItem(`sol-${pageId}`);
			revealed = [];
			initialSetup();
			return false;
		};
		document.getElementById('sol-back').onclick = function()
		{
			if (revealed.length < 1)
				return false;
			revealed.pop();
			localStorage.setItem(`sol-${pageId}`, JSON.stringify(revealed));
			initialSetup();
			return false;
		};
		document.getElementById('sol-forward').onclick = function()
		{
			let btn = document.querySelector('button.reveal');
			if (btn)
				btn.onclick();
			return false;
		};
		document.getElementById('sol-expand').onclick = function()
		{
			let btn, prev;
			while ((btn = document.querySelector('button.reveal')) && btn !== prev)
			{
				btn.onclick();
				prev = btn;
			}
			return false;
		};

		if (triggers && triggers.reset)
			triggers.reset();

		let revealedRaw, revealedTemp;
		if (localStorage && (revealedRaw = localStorage.getItem(`sol-${pageId}`)) && (revealedTemp = JSON.parse(revealedRaw)))
		{
			revealing = true;
			revealed = [];
			for (let rId of revealedTemp)
			{
				let elem = document.getElementById(`b-${rId}`);
				if (elem)
				{
					elem.onclick();
					revealed.push(rId);
				}
			}
			revealing = false;
		}
	}
	initialSetup();
}

function createDeductionTracker(id, pages, fnc)
{
	let lsid = `sol-${id}-deduction`;
	deductionTrackers.push(lsid);

	return function()
	{
		let elem = document.getElementById(id);
		elem.classList.add('deduction-tracker');
		elem.innerHTML = `
			<button type='button' class='far-left'></button>
			<button type='button' class='left' accesskey=','></button>
			<button type='button' class='right' accesskey='/'></button>
			<button type='button' class='far-right'></button>
			<div class='info'></div>
			<div class='progressbg'></div>
			<div class='progress'></div>
		`;

		let curPage = 0;

		if (localStorage)
			curPage = localStorage.getItem(lsid) | 0;

		elem.querySelector('button.far-left').onclick = function()
		{
			curPage = 0;
			showPage();
		}
		elem.querySelector('button.left').onclick = function()
		{
			if (curPage > 0)
				curPage--;
			showPage();
		}
		elem.querySelector('button.right').onclick = function()
		{
			if (curPage < pages.length - 1)
				curPage++;
			showPage();
		}
		elem.querySelector('button.far-right').onclick = function()
		{
			curPage = pages.length - 1;
			showPage();
		}

		function showPage()
		{
			let page = pages[curPage];
			elem.querySelector('.info').innerHTML = page.label || '';
			elem.querySelector('.progress').style.width = `${100*curPage/(pages.length-1)}%`;
			fnc(page, curPage, pages);
			localStorage.setItem(lsid, curPage);
		}
		showPage();
	};
}
