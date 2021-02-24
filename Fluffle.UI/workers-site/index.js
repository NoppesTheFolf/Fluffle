import { getAssetFromKV, serveSinglePageApp } from '@cloudflare/kv-asset-handler'

// Cache for an hour
const browserCacheTtl = 60 * 60;

addEventListener('fetch', event => {
  event.respondWith(handleEvent(event));
})

async function handleEvent(event) {
  let response = await getAsset(event);

  if (!response.headers.get('Content-Type').includes('text/html')) {
    response.headers.set('Cache-Control', 'max-age=' + browserCacheTtl)
  }

  response.headers.set('X-XSS-Protection', '1; mode=block');
  response.headers.set('X-Content-Type-Options', 'nosniff');
  response.headers.set('X-Frame-Options', 'DENY');
  response.headers.set('Referrer-Policy', 'unsafe-url');
  response.headers.set('Feature-Policy', 'none');

  return response;
}

async function getAsset(event) {
  try {
    // See if we can serve the request using a normal 
    return await getAssetFromKV(event, {
      mapRequestToAsset: () => serveSinglePageApp(event.request)
    });
  } catch {
    // If the requested is not found for example, we serve the SPA its 404 page
    return await getAssetFromKV(event, {
      mapRequestToAsset: req => serveSinglePageApp(new Request(`${new URL(req.url).origin}/index.html`, req))
    });
  }
}
