// c = convention
// d = deduction
// o = observation
// r = reveal
// s = strategy

function makeSolutionPage(pageId, hpsml)
{
	hpsml = hpsml
		.replace(/«([^»]*?)»/g, (_, m) => `<span class='w'>${m}</span>`)
		.replace(/‹([^›]*?)›/g, (_, m) => `<span class='h'>${m}</span>`);

	let idAlloc = 1;
	let alloc = {};
	let revealed = [];
	let revealing = false;

	function addAlloc(id, fnc)
	{
		if (!(id in alloc))
			alloc[id] = [];
		alloc[id].push(fnc);
	}

	function makeSolutionPageImpl(hpsml, eof)
	{
		let html = [], fncs = [], m;
		while (m = /^(?<pre>.*?)(?<c>\[(?<sblock>-?)(?<type>[cdors])(?<sid>\w*)(?:\s|(?=\]))|\]|\{(?<cblock>-?)(?<cid>\w*)(?:\/(?<crid>\w+))?(?:\s|(?=\}))|\}|$)/s.exec(hpsml))
		{
			html.push(m.groups.pre);
			hpsml = hpsml.substr(m[0].length);

			let nid;
			switch (m.groups.c[0])
			{
				case ']':
				case '}':
				case undefined:
					if (eof !== m.groups.c[0])
						console.error(`Unexpected ${m.groups.c[0] || 'EOF'} (expected ${eof || 'EOF'})`, m[0]);
					return { html: html.join(''), fncs: function() { fncs.forEach(f => { f(); }); }, rest: hpsml };

				case '[':
					nid = idAlloc++;
					let sid = m.groups.sid;
					html.push(`<button type='button' class='reveal ${m.groups.type}' id='b-${nid}' accesskey='.'></button>`);
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
									addAlloc(re.dataset[`rem_${sid}`], function() { re.classList.remove(`re-${sid}`); });
							});
							let sort = Array.from(document.getElementsByClassName(`sort-${sid}`));
							if (sort.length > 0)
							{
								sort.sort((a, b) => (a.dataset[`sort_${sid}`] | 0) - (b.dataset[`sort_${sid}`] | 0));
								let parentNode = sort[0].parentNode;
								parentNode.append(...sort);
							}
							if (!revealing)
							{
								revealed.push(nid);
								localStorage.setItem(`sol-${pageId}`, JSON.stringify(revealed));
							}
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

		document.getElementById('solution').innerHTML = `
			<div id='sol-controls'>
				<button id='sol-back' accesskey='w'>◀◀</button>
				<button id='sol-reset' accesskey='r'>Reset</button>
				<button id='sol-expand' accesskey='a'>Expand all</button>
			</div>
			${result.html}`;
		result.fncs();

		document.getElementById('sol-back').onclick = function()
		{
			if (revealed.length < 1)
				return false;
			revealed.pop();
			localStorage.setItem(`sol-${pageId}`, JSON.stringify(revealed));
			initialSetup();
			return false;
		}
		document.getElementById('sol-reset').onclick = function()
		{
			localStorage.removeItem(`sol-${pageId}`);
			revealed = [];
			initialSetup();
			return false;
		};

		document.getElementById('sol-expand').onclick = function()
		{
			let elem;
			while (elem = document.querySelector('button.reveal'))
				elem.onclick();
			return false;
		};

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