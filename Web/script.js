
var config = {
	width: 960, 
	height: 600,
	params: { enableDebugging:"0" }
	
};
var u = new UnityObject2(config);

jQuery(function() {
	var $player = jQuery("#unityPlayer");
	var $missingScreen = $player.find(".missing");
	var $brokenScreen = $player.find(".broken");
	$missingScreen.hide();
	$brokenScreen.hide();

	u.observeProgress(function (progress) {
		switch(progress.pluginStatus) {
			case "broken":
				$brokenScreen.find("a").click(function (e) {
					e.stopPropagation();
					e.preventDefault();
					u.installPlugin();
					return false;
				});
				$brokenScreen.show();
			break;
			case "missing":
				$missingScreen.find("a").click(function (e) {
					e.stopPropagation();
					e.preventDefault();
					u.installPlugin();
					return false;
				});
				$missingScreen.show();
			break;
			case "installed":
				$missingScreen.remove();
			break;
			case "first":
			break;
		}
	});
	u.initPlugin(jQuery("#unityPlayer")[0], "catfight-royale.unity3d");
});

var _paq = _paq || [];
_paq.push(['trackPageView']);
_paq.push(['enableLinkTracking']);
(function() {
	var u=(("https:" == document.location.protocol) ? "https" : "http") + "://piwik.bloutiouf.com/";
	_paq.push(['setTrackerUrl', u+'piwik.php']);
	_paq.push(['setSiteId', 2]);
	var d=document, g=d.createElement('script'), s=d.getElementsByTagName('script')[0]; g.type='text/javascript';
	g.defer=true; g.async=true; g.src=u+'piwik.js'; s.parentNode.insertBefore(g,s);
})();